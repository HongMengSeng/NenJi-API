const api = require('../../utils/api').api || require('../../utils/api');
const { orderTimer } = require('../../utils/order-timer');

// 订单超时时间（分钟）
const ORDER_TIMEOUT_MINUTES = 30;

/**
 * 安全解析日期（兼容 iOS）
 * iOS 不支持 "yyyy-MM-dd HH:mm:ss" 格式，需要替换为 "yyyy/MM/dd HH:mm:ss"
 * @param {string} dateStr - 日期字符串
 * @returns {Date} - Date 对象
 */
function safeDate(dateStr) {
  if (!dateStr) return new Date(0);
  // 将 "2026-05-06 20:43:28" 转换为 "2026/05/06 20:43:28"
  const normalized = String(dateStr).replace(/-/g, '/');
  return new Date(normalized);
}

Page({
  data: {
    activeTab: 'all',
    currentOrderType: '',
    scrollToView: '',
    page: 1,
    pageSize: 10,
    hasMore: true,
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
    loading: true,
    isRequesting: false,
    isPageVisible: false,
    orderCountdowns: {}
  },

  searchTimer: null,
  countdownTimer: null,
  refreshTimer: null,

  onLoad(options) {
    this.initPageState();
    console.log('Orders page onLoad, options:', options);
    let tab = 'all';
    if (options.tab) {
      console.log('Setting activeTab to:', options.tab);
      tab = options.tab;
    }
    console.log('Current activeTab:', tab);

    this.setData({
      activeTab: tab,
      scrollToView: 'tab-' + tab,
      isPageVisible: true
    });
    this.getOrders();
  },

  onShow() {
    this.initPageState();
    this.setData({ isPageVisible: true });
    this.startCountdownUpdate();
    this.startOrderRefresh();
    // 只在有数据时静默刷新，无数据时走正常加载
    if (this.data.orders.length > 0) {
      // 如果已经有数据且不在加载中，静默刷新第一页数据（不去重，不重置页码）
      if (!this.data.isRequesting) {
        this.refreshOrders();
      }
    } else {
      this.getOrders();
    }
  },

  // 页面状态初始化函数
  initPageState() {
    if (!this.processingTimeoutOrders) {
      this.processingTimeoutOrders = new Set();
    } else {
      this.processingTimeoutOrders.clear();
    }
    console.log('Page state initialized');
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
      fail: () => {
        wx.reLaunch({
          url: '/pages/index/index'
        });
      }
    });
  },

  onSearchInput(e) {
    const keyword = e.detail.value;
    this.setData({ searchKeyword: keyword });
    this.applyLocalSearchFilter(keyword);
  },

  applyLocalSearchFilter: function (keyword) {
    const filteredOrders = this.filterOrders(this.data.allOrders, keyword);
    const hasSearchKeyword = keyword && keyword.trim();
    const noSearchResult = hasSearchKeyword && filteredOrders.length === 0 && this.data.allOrders.length > 0;

    this.setData({
      orders: filteredOrders,
      noSearchResult: noSearchResult
    });
  },

  searchOrders() {
    if (this.data.allOrders && this.data.allOrders.length > 0) {
      this.applyLocalSearchFilter(this.data.searchKeyword);
    } else {
      this.getOrders();
    }
  },

  processImageUrl(imageUrl) {
    const utils = require('../../utils/utils');
    return utils.media.processUrl(imageUrl);
  },

  filterOrders: function (orders, keyword) {
    if (!keyword || !keyword.trim()) {
      return orders;
    }

    const searchKey = keyword.trim().toLowerCase();
    return orders.filter(order => {
      if (order.orderNumber && String(order.orderNumber).toLowerCase().includes(searchKey)) {
        return true;
      }
      if (order.typeText && order.typeText.toLowerCase().includes(searchKey)) {
        return true;
      }
      if (order.items && order.items.length > 0) {
        for (let item of order.items) {
          if (item.name && item.name.toLowerCase().includes(searchKey)) {
            return true;
          }
        }
      }
      return false;
    });
  },

  /**
   * 统一响应解析：兼容新API格式 { code: 0, data: { orders: [], total, page, pageSize, totalPages } }
   * 以及旧格式：直接数组 或 { orders: [] }
   */
  _normalizeOrderList(data) {
    if (!data) return { orders: [], total: 0 };
    // 直接数组
    if (Array.isArray(data)) return { orders: data, total: data.length };
    // 新API: { orders: [...], total, page, pageSize, totalPages }
    if (data.orders && Array.isArray(data.orders)) {
      return {
        orders: data.orders,
        total: data.total || data.orders.length,
        page: data.page,
        pageSize: data.pageSize,
        totalPages: data.totalPages
      };
    }
    // 旧兼容: { data: [...] }
    if (data.data && Array.isArray(data.data)) {
      return { orders: data.data, total: data.data.length };
    }
    // 旧兼容: { data: { orders: [...] } }
    if (data.data && data.data.orders && Array.isArray(data.data.orders)) {
      return {
        orders: data.data.orders,
        total: data.data.total || data.data.orders.length
      };
    }
    return { orders: [], total: 0 };
  },

  _mapOrder(order) {
    let typeText = order.typeText || '订单';
    if (!order.typeText) {
      if (order.type === 'goods') typeText = '商品订单';
      else if (order.type === 'food') typeText = '点餐订单';
      else if (order.type === 'activity') typeText = '活动订单';
      else if (order.type === 'acre') typeText = '认购订单';
    }

    // 处理桌号：兼容多种后端字段名
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

  // 获取订单列表（首次加载/刷新/切tab — 始终替换数据）
  getOrders() {
    if (this.data.isRequesting) {
      console.log('请求中，忽略重复调用');
      return;
    }

    this.setData({
      isRequesting: true,
      loading: true,
      searching: true,
      page: 1,
      hasMore: true,
      allOrders: [],
      orders: []
    });

    this._fetchOrders({ page: 1, pageSize: this.data.pageSize }, true);
  },

  // 静默刷新（onShow时）— 更新第一页数据，保留其他页
  refreshOrders() {
    if (this.data.isRequesting) return;
    console.log('[refreshOrders] 静默刷新，当前页:', this.data.page, '已有:', this.data.allOrders.length, '条');
    this.setData({ isRequesting: true });
    // 刷新时更新第一页数据，与已加载的其他页数据合并
    // 保持当前页码不变，因为其他页数据仍然有效
    this._fetchOrders({ page: 1, pageSize: this.data.pageSize }, false);
  },

  // 核心请求方法
  _fetchOrders(params, isReplace) {
    if (this.data.activeTab !== 'all') {
      params.status = this.data.activeTab;
    }

    const self = this;
    api.order.getList(params)
      .then((responseData) => {
        const { orders: rawOrders, total } = self._normalizeOrderList(responseData);
        console.log('[订单列表] 页码:', params.page, '返回条数:', rawOrders.length, 'total:', total);

        const mappedOrders = rawOrders.map(order => self._mapOrder(order));
        mappedOrders.sort((a, b) => safeDate(b.createTime) - safeDate(a.createTime));

        let allOrders;
        if (isReplace) {
          allOrders = mappedOrders;
        } else {
          // 刷新模式：合并第一页新数据与已加载的其他页数据
          // 用新数据更新已有订单，保留未变更的订单
          const existingMap = new Map(self.data.allOrders.map(o => [o.id, o]));
          mappedOrders.forEach(o => existingMap.set(o.id, o));
          allOrders = Array.from(existingMap.values());
          allOrders.sort((a, b) => safeDate(b.createTime) - safeDate(a.createTime));
        }

        // 计算是否还有更多
        const hasMore = allOrders.length < total && mappedOrders.length >= self.data.pageSize;

        self.initOrderCountdowns(allOrders);

        const filteredOrders = self.filterOrders(allOrders, self.data.searchKeyword);
        const hasSearchKeyword = self.data.searchKeyword && self.data.searchKeyword.trim();
        const noSearchResult = hasSearchKeyword && filteredOrders.length === 0 && allOrders.length > 0;

        self.setData({
          allOrders: allOrders,
          orders: filteredOrders,
          noSearchResult: noSearchResult,
          hasMore: hasMore,
          loading: false,
          searching: false,
          isRequesting: false,
          loadingMore: false,
          // 替换模式：使用请求的页码；刷新模式：保持当前页码（因为其他页数据仍保留）
          page: isReplace ? params.page : self.data.page
        });
      })
      .catch((err) => {
        console.error('获取订单列表失败:', err);
        if (isReplace && self.data.allOrders.length === 0) {
          // 首次加载失败，尝试旧接口
          self.getOrdersLegacy(params);
        } else {
          self.setData({
            loading: false,
            searching: false,
            isRequesting: false,
            loadingMore: false
          });
        }
      });
  },

  // 上拉加载更多
  onReachBottom() {
    console.log('[onReachBottom] isRequesting:', this.data.isRequesting, 'hasMore:', this.data.hasMore, 'loadingMore:', this.data.loadingMore);
    if (this.data.isRequesting || !this.data.hasMore || this.data.loadingMore) {
      return;
    }
    this.loadMoreOrders();
  },

  loadMoreOrders() {
    const nextPage = this.data.page + 1;
    console.log('[loadMoreOrders] 加载第', nextPage, '页，当前已有:', this.data.allOrders.length, '条');
    this.setData({ loadingMore: true, isRequesting: true });

    let params = { page: nextPage, pageSize: this.data.pageSize };
    if (this.data.activeTab !== 'all') {
      params.status = this.data.activeTab;
    }

    const self = this;
    api.order.getList(params)
      .then((responseData) => {
        const { orders: rawOrders, total } = self._normalizeOrderList(responseData);
        console.log('[loadMoreOrders] 返回条数:', rawOrders.length, 'total:', total);
        console.log('[loadMoreOrders] 第一页ID:', self.data.allOrders.slice(0, 3).map(o => o.id));
        console.log('[loadMoreOrders] 新页ID:', rawOrders.slice(0, 3).map(o => o.id || o.orderId));

        if (rawOrders.length === 0) {
          self.setData({
            hasMore: false,
            loadingMore: false,
            isRequesting: false
          });
          return;
        }

        const mappedOrders = rawOrders.map(order => self._mapOrder(order));

        // ✅ 追加到现有数据
        const allOrders = [...self.data.allOrders, ...mappedOrders];
        allOrders.sort((a, b) => safeDate(b.createTime) - safeDate(a.createTime));

        // 计算是否还有更多：已加载数量 < 总数 且 本页返回满页
        const hasMore = allOrders.length < total && rawOrders.length >= self.data.pageSize;

        console.log('[loadMoreOrders] 合并后总数:', allOrders.length, 'hasMore:', hasMore);

        self.initOrderCountdowns(allOrders);
        const filteredOrders = self.filterOrders(allOrders, self.data.searchKeyword);

        self.setData({
          allOrders: allOrders,
          orders: filteredOrders,
          page: nextPage,
          hasMore: hasMore,
          loadingMore: false,
          isRequesting: false
        });
      })
      .catch((err) => {
        console.error('加载更多订单失败:', err);
        self.setData({
          loadingMore: false,
          isRequesting: false
        });
      });
  },

  // 旧接口兼容方法
  getOrdersLegacy(params) {
    const self = this;
    Promise.all([
      api.order.getCommodityList(params).catch(() => []),
      api.order.getDishList(params).catch(() => []),
      api.order.getActivityList(params).catch(() => [])
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

        const normalizedCommodity = normalizeOrders(commodityOrders);
        const normalizedDish = normalizeOrders(dishOrders);
        const normalizedActivity = normalizeOrders(activityOrders);

        const combinedOrders = [
          ...normalizedCommodity.map(o => ({ ...o, type: 'goods', typeText: '商品订单' })),
          ...normalizedDish.map(o => ({ ...o, type: 'food', typeText: '点餐订单' })),
          ...normalizedActivity.map(o => ({ ...o, type: 'activity', typeText: '活动订单' }))
        ];

        const allOrders = combinedOrders.map(order => self._mapOrder(order));
        allOrders.sort((a, b) => safeDate(b.createTime) - safeDate(a.createTime));
        self.initOrderCountdowns(allOrders);

        const filteredOrders = self.filterOrders(allOrders, self.data.searchKeyword);
        const hasSearchKeyword = self.data.searchKeyword && self.data.searchKeyword.trim();
        const noSearchResult = hasSearchKeyword && filteredOrders.length === 0 && allOrders.length > 0;

        self.setData({
          allOrders: allOrders,
          orders: filteredOrders,
          noSearchResult: noSearchResult,
          hasMore: false, // 旧接口不支持分页
          loading: false,
          searching: false,
          isRequesting: false
        });
      })
      .catch((err) => {
        console.error('获取订单列表失败:', err);
        self.setData({
          loading: false,
          searching: false,
          isRequesting: false
        });
      });
  },

  mapStatusToText(order) {
    // 新订单聚合API直接返回 status 字段
    if (order.status) {
      const statusMap = {
        'pending': '待付款',
        'paid': '待发货',
        'shipping': '运输中',
        'completed': '已完成',
        'cancelled': '已取消',
        // 点餐订单状态
        'ordered': '已付款',
        // 活动订单状态
        'verify_pending': '待核销',
        'verified': '已核销',
        // 退款状态
        'refund': '退款中',
        'refunded': '已退款'
      };
      return statusMap[order.status] || order.statusText || '进行中';
    }

    // 旧接口兼容
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
      activeTab: tab,
      scrollToView: 'tab-' + tab,
      searchKeyword: '',
      noSearchResult: false,
      page: 1,
      hasMore: true,
      allOrders: [],
      orders: []
    });
    this.getOrders();
  },

  initOrderCountdowns(orders) {
    const orderCountdowns = {};
    orders.forEach(order => {
      if (order.status === 'pending' || order.status === 'pending_payment') {
        const remaining = orderTimer.getRemainingTime(order.createTime);
        if (remaining > 0) {
          orderCountdowns[order.id] = orderTimer.formatTime(remaining);
        } else {
          orderCountdowns[order.id] = '00:00';
          this.handleOrderTimeout(order.id);
        }
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

          if (remaining <= 0) {
            this.handleOrderTimeout(order.id);
          }
        }
      });

      if (hasChange) {
        this.setData({ orderCountdowns: nextCountdowns });
      }
    }, 1000);
  },

  stopCountdownUpdate() {
    if (this.countdownTimer) {
      clearInterval(this.countdownTimer);
      this.countdownTimer = null;
    }
  },

  handleOrderTimeout(orderId) {
    if (this.processingTimeoutOrders && this.processingTimeoutOrders.has(orderId)) {
      return;
    }

    this.processingTimeoutOrders.add(orderId);
    console.log('处理超时订单:', orderId);

    orderTimer.handleTimeout(orderId, (id) => {
      console.log('订单超时处理完成，刷新列表:', id);
      this.getOrders();
    });
  },

  startOrderRefresh() {
    this.stopOrderRefresh();
    this.refreshTimer = setInterval(() => {
      if (!this.data.isRequesting && this.data.isPageVisible) {
        this.refreshOrders();
      }
    }, 15000);
  },

  stopOrderRefresh() {
    if (this.refreshTimer) {
      clearInterval(this.refreshTimer);
      this.refreshTimer = null;
    }
  },

  payOrder(e) {
    const { orderId, type, activityid } = e.currentTarget.dataset;
    const id = orderId || e.currentTarget.dataset.id;
    wx.navigateTo({
      url: `/user-pages/pay/pay?orderId=${id}&type=${type}&activityId=${activityid || ''}`
    });
  },

  cancelOrder(e) {
    const id = e.currentTarget.dataset.orderId || e.currentTarget.dataset.id;
    wx.showModal({
      title: '取消订单',
      content: '确定要取消这个订单吗？',
      success: (res) => {
        if (res.confirm) {
          api.order.cancel(id)
            .then(() => {
              wx.showToast({ title: '订单已取消', icon: 'success' });
              this.getOrders();
            })
            .catch(err => {
              wx.showToast({ title: err.message || '取消失败', icon: 'none' });
            });
        }
      }
    });
  },

  deleteOrder(e) {
    const id = e.currentTarget.dataset.orderId || e.currentTarget.dataset.id;
    wx.showModal({
      title: '删除订单',
      content: '确定要删除这个订单记录吗？',
      success: (res) => {
        if (res.confirm) {
          api.order.delete(id)
            .then(() => {
              wx.showToast({ title: '已删除', icon: 'success' });
              this.getOrders();
            })
            .catch(err => {
              wx.showToast({ title: err.message || '删除失败', icon: 'none' });
            });
        }
      }
    });
  },

  deleteCancelledOrder(e) {
    this.deleteOrder(e);
  },

  viewLogistics(e) {
    const id = e.currentTarget.dataset.orderId || e.currentTarget.dataset.id;
    wx.navigateTo({
      url: `/user-pages/logistics-detail/logistics-detail?orderId=${id}`
    });
  },

  viewOrderDetail(e) {
    const id = e.currentTarget.dataset.orderId || e.currentTarget.dataset.id;
    wx.navigateTo({
      url: `/user-pages/orders-detail/orders-detail?id=${id}`
    });
  },

  viewDetail(e) {
    this.viewOrderDetail(e);
  },

  goToShop() {
    wx.reLaunch({
      url: '/pages/index/index'
    });
  },

  onPullDownRefresh() {
    this.setData({
      page: 1,
      hasMore: true
    });
    this.getOrders();
    setTimeout(() => {
      wx.stopPullDownRefresh();
    }, 1000);
  }
});
