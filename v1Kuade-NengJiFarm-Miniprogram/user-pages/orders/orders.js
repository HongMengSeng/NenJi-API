const api = require('../../utils/api').api || require('../../utils/api');
const { orderTimer } = require('../../utils/order-timer');

const PAGE_SIZE = 10;

function safeDate(dateStr) {
  if (!dateStr) return new Date(0);
  const normalized = String(dateStr).replace(/-/g, '/');
  return new Date(normalized);
}

Page({
  data: {
    activeTab: 'all',
    currentOrderType: '',
    scrollToView: '',
    currentPage: 1,
    pageSize: PAGE_SIZE,
    totalOrders: 0,
    hasMore: true,
    loading: true,
    loadingMore: false,
    tabs: [
      { key: 'all', name: '全部' },
      { key: 'pending', name: '待付款' },
      { key: 'paid', name: '待发货' },
      { key: 'shipping', name: '待收货' },
      { key: 'cancelled', name: '已取消' }
    ],
    searchKeyword: '',
    searching: false,
    allOrders: [],
    orders: [],
    noSearchResult: false,
    isRequesting: false,
    isPageVisible: false,
    orderCountdowns: {}
  },

  searchTimer: null,
  countdownTimer: null,
  refreshTimer: null,

  onLoad(options) {
    this.initPageState();
    let tab = 'all';
    if (options.tab) tab = options.tab;
    this.setData({ activeTab: tab, scrollToView: 'tab-' + tab, isPageVisible: true });
    this.getOrders();
  },

  onShow() {
    this.initPageState();
    this.setData({ isPageVisible: true });
    this.startCountdownUpdate();
    this.startOrderRefresh();
    if (this.data.orders.length > 0) {
      if (!this.data.isRequesting) this.refreshOrders();
    } else {
      this.getOrders();
    }
  },

  initPageState() {
    if (!this.processingTimeoutOrders) {
      this.processingTimeoutOrders = new Set();
    } else {
      this.processingTimeoutOrders.clear();
    }
  },

  onHide() {
    this.setData({ isPageVisible: false });
    this.stopCountdownUpdate();
    this.stopOrderRefresh();
  },

  onUnload() {
    this.stopCountdownUpdate();
    this.stopOrderRefresh();
  },

  goBack() {
    wx.navigateBack({
      fail: () => { wx.reLaunch({ url: '/pages/index/index' }); }
    });
  },

  onSearchInput(e) {
    const keyword = e.detail.value;
    this.setData({ searchKeyword: keyword });
    this.applyLocalSearchFilter(keyword);
  },

  applyLocalSearchFilter(keyword) {
    const filteredOrders = this.filterOrders(this.data.allOrders, keyword);
    const hasSearchKeyword = keyword && keyword.trim();
    const noSearchResult = hasSearchKeyword && filteredOrders.length === 0 && this.data.allOrders.length > 0;
    const displayOrders = hasSearchKeyword
      ? filteredOrders
      : this.data.allOrders.slice(0, this.data.currentPage * this.data.pageSize);
    this.setData({ orders: displayOrders, noSearchResult: noSearchResult });
  },

  searchOrders() {
    if (this.data.allOrders.length > 0) {
      this.applyLocalSearchFilter(this.data.searchKeyword);
    } else {
      this.getOrders();
    }
  },

  processImageUrl(imageUrl) {
    const utils = require('../../utils/utils');
    return utils.media.processUrl(imageUrl);
  },

  filterOrders(orders, keyword) {
    if (!keyword || !keyword.trim()) return orders;
    const searchKey = keyword.trim().toLowerCase();
    return orders.filter(order => {
      if (order.orderNumber && String(order.orderNumber).toLowerCase().includes(searchKey)) return true;
      if (order.typeText && order.typeText.toLowerCase().includes(searchKey)) return true;
      if (order.items && order.items.length > 0) {
        for (let item of order.items) {
          if (item.name && item.name.toLowerCase().includes(searchKey)) return true;
        }
      }
      return false;
    });
  },

  _normalizeOrderList(data) {
    if (!data) return { orders: [], total: 0, totalPages: 0 };
    if (Array.isArray(data)) return { orders: data, total: data.length, totalPages: 1 };
    if (data.orders && Array.isArray(data.orders)) {
      const total = data.total !== undefined && data.total !== null ? data.total : data.orders.length;
      const totalPages = data.totalPages !== undefined && data.totalPages !== null
        ? data.totalPages
        : Math.max(Math.ceil(total / (data.pageSize || PAGE_SIZE)) || 1, 1);
      return {
        orders: data.orders,
        total,
        page: data.page || 1,
        pageSize: data.pageSize || PAGE_SIZE,
        totalPages
      };
    }
    if (data.data && Array.isArray(data.data)) return { orders: data.data, total: data.data.length, totalPages: 1 };
    if (data.data && data.data.orders && Array.isArray(data.data.orders)) {
      const total = data.data.total !== undefined && data.data.total !== null ? data.data.total : data.data.orders.length;
      const pageSize = data.data.pageSize || PAGE_SIZE;
      return {
        orders: data.data.orders,
        total,
        page: data.data.page || 1,
        pageSize,
        totalPages: data.data.totalPages || Math.max(Math.ceil(total / pageSize) || 1, 1)
      };
    }
    return { orders: [], total: 0, totalPages: 0 };
  },

  _mapOrder(order) {
    let typeText = order.typeText || '订单';
    if (!order.typeText) {
      if (order.type === 'goods') typeText = '商品订单';
      else if (order.type === 'food') typeText = '点餐订单';
      else if (order.type === 'activity') typeText = '活动订单';
      else if (order.type === 'acre') typeText = '认购订单';
    }
    const tableNo = order.diningTableNo || order.tableNumber || order.tableNo || order.dining_table_no || '';
    return {
      ...order,
      id: order.id || order.orderId,
      orderNumber: order.orderNumber || order.orderNo,
      typeText: typeText,
      tableNo: tableNo,
      totalPrice: (order.totalPrice || order.totalAmount || 0).toString().replace(/[¥￥]/g, ''),
      statusText: this.mapStatusToText(order),
      items: (order.items || []).map(item => ({
        ...item,
        image: this.processImageUrl(item.image),
        price: (item.price || item.unitPrice || 0).toString().replace(/[¥￥]/g, '')
      }))
    };
  },

  // ======== 分页加载核心 ========

  // 首次加载 / 切换tab
  getOrders() {
    if (this.data.isRequesting) return;
    this.setData({
      isRequesting: true, loading: true, searching: true,
      currentPage: 1, hasMore: true, allOrders: [], orders: []
    });
    this._fetchPage(1);
  },

  // 静默刷新
  refreshOrders() {
    if (this.data.isRequesting) return;
    this.setData({ isRequesting: true });
    this._fetchPage(1);
  },

  // 加载指定页
  _fetchPage(page) {
    let params = { page, pageSize: PAGE_SIZE };
    if (this.data.activeTab !== 'all') params.status = this.data.activeTab;

    const self = this;
    api.order.getList(params)
      .then((responseData) => {
        const { orders: rawOrders, total } = self._normalizeOrderList(responseData);

        const mappedOrders = rawOrders.map(order => self._mapOrder(order));
        mappedOrders.sort((a, b) => safeDate(b.createTime) - safeDate(a.createTime));

        // 合并到 allOrders（去重）
        const existingIds = new Set(self.data.allOrders.map(o => o.id));
        const newOrders = mappedOrders.filter(o => !existingIds.has(o.id));
        const newAllOrders = [...self.data.allOrders, ...newOrders];
        newAllOrders.sort((a, b) => safeDate(b.createTime) - safeDate(a.createTime));

        self.initOrderCountdowns(newAllOrders);

        // 当前页显示：取前 page * PAGE_SIZE 条
        const displayOrders = newAllOrders.slice(0, page * PAGE_SIZE);
        const hasMore = total > page * PAGE_SIZE;

        self.setData({
          allOrders: newAllOrders,
          orders: displayOrders,
          totalOrders: total,
          hasMore: hasMore,
          currentPage: page,
          loading: false,
          searching: false,
          isRequesting: false,
          loadingMore: false
        });
      })
      .catch((err) => {
        console.error('获取订单列表失败:', err);
        if (page === 1 && self.data.allOrders.length === 0) {
          self.getOrdersLegacy();
        } else {
          self.setData({ loading: false, searching: false, isRequesting: false, loadingMore: false });
        }
      });
  },

  // 加载下一页
  loadNextPage() {
    const nextPage = this.data.currentPage + 1;
    if (this.data.isRequesting || !this.data.hasMore) return;
    this.setData({ loadingMore: true, isRequesting: true });

    let params = { page: nextPage, pageSize: PAGE_SIZE };
    if (this.data.activeTab !== 'all') params.status = this.data.activeTab;

    const self = this;
    api.order.getList(params)
      .then((responseData) => {
        const { orders: rawOrders, total } = self._normalizeOrderList(responseData);

        if (rawOrders.length === 0) {
          self.setData({ hasMore: false, loadingMore: false, isRequesting: false });
          return;
        }

        const mappedOrders = rawOrders.map(order => self._mapOrder(order));

        const existingIds = new Set(self.data.allOrders.map(o => o.id));
        const newOrders = mappedOrders.filter(o => !existingIds.has(o.id));
        const newAllOrders = [...self.data.allOrders, ...newOrders];
        newAllOrders.sort((a, b) => safeDate(b.createTime) - safeDate(a.createTime));

        const hasMore = total > nextPage * PAGE_SIZE;

        self.initOrderCountdowns(newAllOrders);

        // 显示到当前页的数据
        const displayOrders = newAllOrders.slice(0, nextPage * PAGE_SIZE);

        self.setData({
          allOrders: newAllOrders,
          orders: displayOrders,
          totalOrders: total,
          currentPage: nextPage,
          hasMore: hasMore,
          loadingMore: false,
          isRequesting: false
        });
      })
      .catch((err) => {
        console.error('加载下一页失败:', err);
        self.setData({ loadingMore: false, isRequesting: false });
      });
  },

  // 加载上一页（直接从 allOrders 缓存中取）
  loadPrevPage() {
    const prevPage = this.data.currentPage - 1;
    if (prevPage < 1) return;

    const displayOrders = this.data.allOrders.slice(0, prevPage * PAGE_SIZE);

    this.setData({
      currentPage: prevPage,
      orders: displayOrders,
      loadingMore: false,
      isRequesting: false
    });
    wx.pageScrollTo({ scrollTop: 0, duration: 200 });
  },

  // 触底自动加载（只触发下一页，不触发上一页）
  onReachBottom() {
    if (!this.data.hasMore || this.data.isRequesting || this.data.loadingMore) return;
    // 只有当当前已显示数据达到当前页的末端时才触发
    const loadedCount = this.data.orders.length;
    const currentMax = this.data.currentPage * PAGE_SIZE;
    if (loadedCount >= currentMax && loadedCount > 0) {
      this.loadNextPage();
    }
  },

  // 旧接口兼容
  getOrdersLegacy() {
    const self = this;
    Promise.all([
      api.order.getCommodityList({ page: 1, pageSize: PAGE_SIZE }).catch(() => []),
      api.order.getDishList({ page: 1, pageSize: PAGE_SIZE }).catch(() => []),
      api.order.getActivityList({ page: 1, pageSize: PAGE_SIZE }).catch(() => [])
    ])
      .then(([commodityOrders, dishOrders, activityOrders]) => {
        const normalizeOrders = (data) => {
          if (!data) return [];
          if (Array.isArray(data)) return data;
          if (data.orders && Array.isArray(data.orders)) return data.orders;
          if (data.data && Array.isArray(data.data)) return data.data;
          if (typeof data === 'object') {
            const arr = Object.values(data).find(v => Array.isArray(v));
            return arr || [];
          }
          return [];
        };
        const combinedOrders = [
          ...normalizeOrders(commodityOrders).map(o => ({ ...o, type: 'goods', typeText: '商品订单' })),
          ...normalizeOrders(dishOrders).map(o => ({ ...o, type: 'food', typeText: '点餐订单' })),
          ...normalizeOrders(activityOrders).map(o => ({ ...o, type: 'activity', typeText: '活动订单' }))
        ];
        const allOrders = combinedOrders.map(order => self._mapOrder(order));
        allOrders.sort((a, b) => safeDate(b.createTime) - safeDate(a.createTime));
        self.initOrderCountdowns(allOrders);

        const pageOrders = allOrders.slice(0, PAGE_SIZE);
        self.setData({
          allOrders: allOrders,
          orders: pageOrders,
          totalOrders: allOrders.length,
          hasMore: allOrders.length > PAGE_SIZE,
          loading: false, searching: false, isRequesting: false
        });
      })
      .catch((err) => {
        console.error('获取订单列表失败:', err);
        self.setData({ loading: false, searching: false, isRequesting: false });
      });
  },

  mapStatusToText(order) {
    if (order.status) {
      const statusMap = {
        'pending': '待付款', 'paid': '待发货', 'shipping': '运输中',
        'completed': '已完成', 'cancelled': '已取消',
        'ordered': '已付款', 'verify_pending': '待核销', 'verified': '已核销',
        'refund': '退款中', 'refunded': '已退款'
      };
      return statusMap[order.status] || order.statusText || '进行中';
    }
    const statusId = order.orderStatusId || order.orderStatus;
    const type = order.type;
    if (type === 'goods') {
      const statusMap = { 1: '待付款', 2: '待发货', 3: '运输中', 4: '已完成', 5: '已取消', 6: '退款中', 7: '已退款' };
      return statusMap[statusId] || '未知状态';
    } else if (type === 'food') {
      const statusMap = { 1: '待付款', 2: '已付款', 3: '已完成', 4: '已取消' };
      return statusMap[statusId] || '未知状态';
    } else if (type === 'activity') {
      const statusMap = { 1: '待付款', 2: '待核销', 3: '已核销', 4: '已取消', 5: '退款中', 6: '已退款' };
      return statusMap[statusId] || '未知状态';
    }
    return order.statusText || '进行中';
  },

  switchTab(e) {
    const tab = e.currentTarget.dataset.tab;
    if (tab === this.data.activeTab) return;
    this.setData({
      activeTab: tab, scrollToView: 'tab-' + tab,
      searchKeyword: '', noSearchResult: false,
      currentPage: 1, hasMore: true,
      allOrders: [], orders: []
    });
    wx.pageScrollTo({ scrollTop: 0, duration: 0 });
    this.getOrders();
  },

  initOrderCountdowns(orders) {
    const orderCountdowns = {};
    orders.forEach(order => {
      if (order.status === 'pending' || order.status === 'pending_payment') {
        const remaining = orderTimer.getRemainingTime(order.createTime);
        orderCountdowns[order.id] = remaining > 0 ? orderTimer.formatTime(remaining) : '00:00';
        if (remaining <= 0) this.handleOrderTimeout(order.id);
      }
    });
    this.setData({ orderCountdowns });
  },

  startCountdownUpdate() {
    this.stopCountdownUpdate();
    this.countdownTimer = setInterval(() => {
      const { orders, orderCountdowns } = this.data;
      const nextCountdowns = { ...orderCountdowns };
      let hasChange = false;
      orders.forEach(order => {
        if (order.status === 'pending' || order.status === 'pending_payment') {
          const remaining = orderTimer.getRemainingTime(order.createTime);
          const nextTimeStr = orderTimer.formatTime(remaining);
          if (nextCountdowns[order.id] !== nextTimeStr) {
            nextCountdowns[order.id] = nextTimeStr;
            hasChange = true;
          }
          if (remaining <= 0) this.handleOrderTimeout(order.id);
        }
      });
      if (hasChange) this.setData({ orderCountdowns: nextCountdowns });
    }, 1000);
  },

  stopCountdownUpdate() {
    if (this.countdownTimer) { clearInterval(this.countdownTimer); this.countdownTimer = null; }
  },

  handleOrderTimeout(orderId) {
    if (this.processingTimeoutOrders && this.processingTimeoutOrders.has(orderId)) return;
    this.processingTimeoutOrders.add(orderId);
    orderTimer.handleTimeout(orderId, (id) => { this.getOrders(); });
  },

  startOrderRefresh() {
    this.stopOrderRefresh();
    this.refreshTimer = setInterval(() => {
      if (!this.data.isRequesting && this.data.isPageVisible) this.refreshOrders();
    }, 15000);
  },

  stopOrderRefresh() {
    if (this.refreshTimer) { clearInterval(this.refreshTimer); this.refreshTimer = null; }
  },

  payOrder(e) {
    const id = e.currentTarget.dataset.orderId || e.currentTarget.dataset.id;
    wx.navigateTo({ url: `/user-pages/pay/pay?orderId=${id}&type=${e.currentTarget.dataset.type || 'goods'}` });
  },

  cancelOrder(e) {
    const id = e.currentTarget.dataset.orderId || e.currentTarget.dataset.id;
    wx.showModal({
      title: '取消订单',
      content: '确定要取消这个订单吗？',
      success: (res) => {
        if (res.confirm) {
          api.order.cancel(id).then(() => {
            wx.showToast({ title: '订单已取消', icon: 'success' });
            this.getOrders();
          }).catch(err => wx.showToast({ title: err.message || '取消失败', icon: 'none' }));
        }
      }
    });
  },

  viewLogistics(e) {
    const id = e.currentTarget.dataset.orderId || e.currentTarget.dataset.id;
    wx.navigateTo({ url: `/user-pages/logistics-detail/logistics-detail?orderId=${id}` });
  },

  viewOrderDetail(e) {
    const id = e.currentTarget.dataset.orderId || e.currentTarget.dataset.id;
    wx.navigateTo({ url: `/user-pages/orders-detail/orders-detail?id=${id}` });
  },

  viewDetail(e) { this.viewOrderDetail(e); },

  goToShop() { wx.reLaunch({ url: '/pages/index/index' }); },

  onPullDownRefresh() {
    this.getOrders();
    setTimeout(() => { wx.stopPullDownRefresh(); }, 1000);
  }
});
