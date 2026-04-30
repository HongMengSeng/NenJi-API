const api = require('../../utils/api');

Page({
  data: {
    activeCategory: 'vegetables',
    categories: [],
    goodsList: {},
    mergedGoodsList: [],
    pageMap: {},
    hasMoreMap: {},
    pageSize: 6,

    cart: {},
    cartItems: [],
    cartCount: 0,
    totalPrice: 0,

    loading: true,
    lazyLoading: false,
    showCartModal: false,

    tableNumber: null,
    showTableModal: false,
    tableList: [],

    scrollIntoViewId: '',
    isManualScroll: false
  },

  onLoad(options) {
    if (options.tableId && options.secret) {
      const tableNumber = options.tableId;
      this.setData({ tableNumber });
      wx.setStorageSync('tableNumber', tableNumber);
      setTimeout(() => {
        wx.showToast({ title: `已绑定桌台 ${tableNumber}`, icon: 'success' });
      }, 500);
    } else {
      const cart = wx.getStorageSync('orderCart') || {};
      this.restoreCart(cart);
      const storedTableNumber = wx.getStorageSync('tableNumber');
      if (storedTableNumber) {
        this.setData({ tableNumber: storedTableNumber });
      }
    }

    this.getTableList();
    setTimeout(() => {
      this.getOrderData();
    }, 500);
  },

  onShow() {
    try {
      const cart = wx.getStorageSync('orderCart') || {};
      this.restoreCart(cart);
      const tableNumber = wx.getStorageSync('tableNumber');
      if (tableNumber) this.setData({ tableNumber });
      this.syncFromCart();
    } catch (e) {}
  },

  syncFromCart() {
    try {
      // 不再从 cartList 同步，避免覆盖点餐页面新添加的数据
      // 只保持当前 orderCart 的数据即可
    } catch (e) {}
  },

  getOrderData() {
    wx.showLoading({ title: '加载中...' })
    // 使用正确的菜品分类接口
    api.goods.getCategories({ type: 'food' })
      .then(data => {
        const categories = [
          { id: 'all', name: '全部菜品' },
          ...(data || []).map(cat => ({
            id: String(cat.id),
            name: cat.name
          }))
        ];
        const currentCategory = 'all';
        this.setData({
          activeCategory: currentCategory,
          categories,
          pageMap: { [currentCategory]: 1 },
          hasMoreMap: { [currentCategory]: true },
          loading: false
        });

        this.updateMergedGoodsList();
        this.loadAllCategories();
      })
      .catch(err => {
        console.error('获取分类失败', err);
        this.setData({ loading: false });
        wx.showToast({ title: '加载失败', icon: 'none' });
      })
      .finally(() => wx.hideLoading());
  },

  // 处理图片URL，使用 /api/file/image/ API
  processImageUrl(imageUrl) {
    if (!imageUrl) return '';
    imageUrl = String(imageUrl).replace(/[`\s]/g, '');
    
    // 如果已经是完整的API URL，直接返回
    if (imageUrl.includes('http://192.168.203.56/api/file/image/')) {
      return imageUrl;
    }
    
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      imageUrl = imageUrl.replace('http://127.0.0.1:5000', 'http://192.168.203.56');
      if (imageUrl.includes('/images/farm/')) {
        const fileName = imageUrl.split('/images/farm/')[1];
        return 'http://192.168.203.56/api/file/image/' + fileName;
      }
      return imageUrl;
    }
    
    // 处理相对路径
    let fileName = imageUrl;
    
    // 去除所有开头的斜杠
    while (fileName.startsWith('/')) {
      fileName = fileName.substring(1);
    }
    
    // 提取文件名（去掉路径部分）
    if (fileName.includes('/images/farm/')) {
      fileName = fileName.split('/images/farm/')[1];
    } else if (fileName.includes('images/farm/')) {
      fileName = fileName.split('images/farm/')[1];
    } else if (fileName.includes('/')) {
      // 如果还有路径，只取最后一部分作为文件名
      fileName = fileName.split('/').pop();
    }
    
    // 如果文件名仍然包含路径分隔符，再次处理
    if (fileName.includes('/')) {
      fileName = fileName.split('/').pop();
    }
    
    return 'http://192.168.203.56/api/file/image/' + fileName;
  },

  addImageUrlsToGoods(goods) {
    return goods.map(item => ({
      ...item,
      image: this.processImageUrl(item.image),
      price: (item.price || '').toString().replace(/[¥￥]/g, ''),
      stock: item.stock || 0,
      sold: item.sold || 0
    }));
  },

  switchCategory(e) {
    const category = e.currentTarget.dataset.category;
    this.setData({ isManualScroll: true, activeCategory: category });
    this.updateMergedGoodsList();
    this.setData({ scrollIntoViewId: `category-${category}` });
    setTimeout(() => {
      this.setData({ isManualScroll: false, scrollIntoViewId: '' });
    }, 500);
  },

  onScroll(e) {
    if (this.data.isManualScroll) return;
    const scrollTop = e.detail.scrollTop;
    const query = wx.createSelectorQuery();
    this.data.categories.forEach(item => {
      query.select(`#category-${item.id}`).boundingClientRect()
    });
    query.select('.goods-list').boundingClientRect();
    query.exec(res => {
      const listRect = res[res.length - 1];
      let curId = '';
      for (let i = 0; i < this.data.categories.length; i++) {
        const rect = res[i];
        if (rect && rect.top <= listRect.top + 10) {
          curId = this.data.categories[i].id;
        }
      }
      if (curId && curId !== this.data.activeCategory) {
        this.setData({ activeCategory: curId });
      }
    })
  },

  loadCategoryGoods(category, isLoadMore) {
    if (isLoadMore && this.data.lazyLoading) return;
    const nextPage = isLoadMore ? (this.data.pageMap[category] || 0) + 1 : 1;
    this.setData({ lazyLoading: isLoadMore });

    let reqData = { type: 'food', page: nextPage, pageSize: this.data.pageSize };
    if (category !== 'all') {
      reqData.categoryId = category;
    }

    api.goods.getList(reqData)
      .then(data => {
        const newGoods = this.addImageUrlsToGoods(data || []);
        const old = this.data.goodsList[category] || [];
        this.setData({
          [`goodsList.${category}`]: isLoadMore ? old.concat(newGoods) : newGoods,
          [`pageMap.${category}`]: nextPage,
          [`hasMoreMap.${category}`]: newGoods.length >= this.data.pageSize,
          lazyLoading: false
        });
        this.updateMergedGoodsList();
      }).catch(() => {
        this.setData({ lazyLoading: false });
        wx.showToast({ title: '加载失败', icon: 'none' });
      });
  },

  loadAllCategories() {
    this.data.categories.forEach(c => {
      if (!this.data.goodsList[c.id]) this.loadCategoryGoods(c.id, false);
    });
  },

  updateMergedGoodsList() {
    const { categories, goodsList } = this.data;
    const merged = [];
    categories.forEach(c => {
      merged.push({ type: 'category', id: c.id, name: c.name, uniqueKey: `cat-${c.id}` });
      (goodsList[c.id] || []).forEach(g => merged.push({ ...g, uniqueKey: `g-${g.id}` }));
    });
    this.setData({ mergedGoodsList: merged });
  },

  addToCart(e) {
    const id = e.currentTarget.dataset.id;
    const goods = this.data.mergedGoodsList.find(i => i.id == id);
    if (!goods) return;
    const newCart = { ...this.data.cart };
    const key = String(id);
    if (newCart[key]) {
      if (newCart[key].quantity >= goods.stock) return wx.showToast({ title: '库存不足', icon: 'none' });
      newCart[key].quantity++;
    } else newCart[key] = { ...goods, quantity: 1 };
    this.syncCartState(newCart);
  },

  increaseQuantity(e) {
    const id = e.currentTarget.dataset.id + '';
    const newCart = { ...this.data.cart };
    if (!newCart[id]) return;
    if (newCart[id].quantity >= newCart[id].stock) return wx.showToast({ title: '库存不足', icon: 'none' });
    newCart[id].quantity++;
    this.syncCartState(newCart);
  },

  decreaseQuantity(e) {
    const id = e.currentTarget.dataset.id + '';
    const newCart = { ...this.data.cart };
    if (!newCart[id]) return;
    if (newCart[id].quantity <= 1) delete newCart[id];
    else newCart[id].quantity--;
    this.syncCartState(newCart);
  },

  onQuantityInput(e) {
    const id = e.currentTarget.dataset.id + '';
    const val = Math.max(0, parseInt(e.detail.value) || 0);
    const goods = this.data.mergedGoodsList.find(i => i.id == id);
    if (!goods) return;
    const newCart = { ...this.data.cart };
    if (val === 0) delete newCart[id];
    else newCart[id] = { ...newCart[id], quantity: Math.min(val, goods.stock) };
    this.syncCartState(newCart);
  },

  syncCartState(newCart) {
    let count = 0, total = 0;
    // 保存时添加 checked 状态，默认勾选
    const cartWithChecked = {};
    for (const key in newCart) {
      cartWithChecked[key] = {
        ...newCart[key],
        checked: true // 默认勾选
      };
      count += cartWithChecked[key].quantity;
      total += cartWithChecked[key].price * cartWithChecked[key].quantity;
    }
    this.setData({ cart: newCart, cartItems: Object.values(newCart), cartCount: count, totalPrice: +total.toFixed(2) });
    wx.setStorageSync('orderCart', cartWithChecked);
  },

  restoreCart(cart) {
    let count = 0, total = 0;
    const restoredCart = {};
    Object.values(cart || {}).forEach(i => {
      const key = String(i.id);
      restoredCart[key] = {
        ...i,
        checked: i.checked !== false
      };
      count += i.quantity || i.count || 0;
      total += (i.price || 0) * (i.quantity || i.count || 0);
    });
    this.setData({ cart: restoredCart, cartItems: Object.values(restoredCart), cartCount: count, totalPrice: +total.toFixed(2) });
  },

  viewCart() {
    if (this.data.cartCount === 0) return wx.showToast({ title: '购物车为空', icon: 'none' });
    this.setData({ showCartModal: true });
  },
  hideCartModal() { this.setData({ showCartModal: false }); },

  checkout() {
    if (this.data.cartCount === 0) return wx.showToast({ title: '购物车为空', icon: 'none' });
    if (!this.data.tableNumber) return wx.showToast({ title: '请选择桌台', icon: 'none' });
    wx.navigateTo({ url: '/user-pages/confirm-order/confirm-order' });
  },

  navigateToGoodsDetail(e) {
    const id = e.currentTarget.dataset.id;
    const goods = this.data.mergedGoodsList.find(i => i.id == id);
    if (!goods) return;
    const p = encodeURIComponent(JSON.stringify({ id: goods.id, sold: goods.sold, stock: goods.stock }));
    wx.navigateTo({ url: `/user-pages/order-foods-detail/order-foods-detail?params=${p}` });
  },

  onReachBottom() {
    const cat = this.data.activeCategory;
    if (!this.data.hasMoreMap[cat] || this.data.lazyLoading) return;
    this.loadCategoryGoods(cat, true);
  },

  selectTable() { this.setData({ showTableModal: true }); },
  hideTableModal() { this.setData({ showTableModal: false }); },
  selectTableNumber(e) {
    const id = e.currentTarget.dataset.tableId;
    this.setData({ tableNumber: id, showTableModal: false });
    wx.setStorageSync('tableNumber', id);
  },

  testScanCode() {
    wx.scanCode({
      success: res => {
        const q = res.result.match(/query=([^&]+)/)?.[1];
        if (!q) return wx.showToast({ title: '无效二维码', icon: 'none' });
        const d = Object.fromEntries(q.split('&').map(kv => kv.split('=').map(decodeURIComponent)));
        if (d.tableId) {
          this.setData({ tableNumber: d.tableId });
          wx.setStorageSync('tableNumber', d.tableId);
          wx.showToast({ title: `桌台 ${d.tableId} 绑定成功`, icon: 'success' });
        }
      }
    });
  },

  getTableList() {
    this.setData({ tableList: [1, 2, 3, 4, 5, 6, 7, 8].map(i => ({ id: String(i), name: `桌台${i}` })) });
  },

  stopPropagation() { return false },
  navigateToService() {
    wx.showModal({ title: '客服', content: '电话：15876534944\n微信：njjtnc15876534944', showCancel: false });
  }
});