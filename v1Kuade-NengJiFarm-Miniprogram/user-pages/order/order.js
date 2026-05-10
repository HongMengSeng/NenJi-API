const api = require('../../utils/api');

let globalKeyCounter = 0;

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
      // 已有数据则直接刷新合并列表，避免 onShow 期间出现空白
      if (this.data.categories.length > 0 && Object.keys(this.data.goodsList).length > 0) {
        this.updateMergedGoodsList();
        // 静默刷新库存（不阻塞显示）
        this.refreshStock();
      }

      // 恢复购物车状态
      const cart = wx.getStorageSync('orderCart') || {};
      this.restoreCart(cart);
      const tableNumber = wx.getStorageSync('tableNumber');
      if (tableNumber) this.setData({ tableNumber });
      this.syncFromCart();
    } catch (e) {}
  },

  syncFromCart() {
    try {
      const cart = wx.getStorageSync('orderCart') || {};
      this.restoreCart(cart);
    } catch (e) {
      console.error('syncFromCart error:', e);
    }
  },

  // 获取菜单数据：使用 /api/order/getOrderData 一次性获取全部分类 + 菜品
  getOrderData() {
    wx.showLoading({ title: '加载中...' })
    api.order.getFullMenu()
      .then(data => {
        console.log('菜单原始数据:', JSON.stringify(data).substring(0, 2000));
        // 后端返回格式为 { data: { data: { categories, goodsList } } }
        // request() 已解一层 data，还需再解两层
        const inner = data.data || data;
        const menuData = inner.data || inner;
        let rawCats = [];
        let rawGoodsList = {};

        if (Array.isArray(menuData)) {
          // 格式A: data 本身就是分类数组
          rawCats = menuData;
        } else if (Array.isArray(menuData.categories)) {
          rawCats = menuData.categories;
          rawGoodsList = menuData.goodsList || {};
        } else if (Array.isArray(menuData.list)) {
          rawCats = menuData.list;
          rawGoodsList = menuData.goodsList || {};
        } else if (Array.isArray(menuData.data)) {
          rawCats = menuData.data;
        } else if (Array.isArray(menuData.records)) {
          rawCats = menuData.records;
        } else if (Array.isArray(menuData.items)) {
          rawCats = menuData.items;
        }

        console.log('解析后的分类数:', rawCats.length, 'goodsList 键:', Object.keys(rawGoodsList));

        const categories = [
          { id: 'all', name: '全部菜品' },
          ...rawCats.map(cat => ({
            id: String(cat.id),
            name: cat.name
          }))
        ];

        // 构建 goodsList：按分类 ID 索引
        const goodsList = {};
        const allItems = [];
        const goodsKeys = Object.keys(rawGoodsList);

        if (goodsKeys.length > 0) {
          // 格式1: goodsList 是 { "分类ID": [items] }
          goodsKeys.forEach(catId => {
            const items = this.addImageUrlsToGoods(rawGoodsList[catId] || []);
            goodsList[catId] = items;
            allItems.push(...items);
          });
        } else {
          // 格式2: 从分类中提取 items/dishes/products/goods/list
          rawCats.forEach(cat => {
            const catId = String(cat.id);
            const itemList = cat.items || cat.dishes || cat.products || cat.goods || cat.list || cat.foods || [];
            const items = this.addImageUrlsToGoods(itemList);
            goodsList[catId] = items;
            allItems.push(...items);
          });
        }
        goodsList['all'] = allItems;

        console.log('分类数:', categories.length, '商品总数:', allItems.length);

        this.setData({
          activeCategory: 'all',
          categories,
          goodsList,
          pageMap: {},
          hasMoreMap: {},
          loading: false
        });

        this.updateMergedGoodsList();
      })
      .catch(err => {
        console.error('获取菜单失败:', err);
        this.setData({ loading: false });
        wx.showToast({ title: '加载失败', icon: 'none' });
      })
      .finally(() => wx.hideLoading());
  },

  // 图片处理（沿用对方的工具方法）
  processImageUrl(imageUrl) {
    const utils = require('../../utils/utils');
    return utils.media.processUrl(imageUrl);
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
    const query = wx.createSelectorQuery();
    
    this.data.categories.forEach(item => {
      query.select(`#category-${item.id}`).boundingClientRect()
    });
    query.select('.goods-list').boundingClientRect();
  
    query.exec(res => {
      const listRect = res[res.length - 1];
      let curId = this.data.activeCategory;
  
      // 👇 核心终极逻辑：只要上一个分类完全跑出去，立刻切下一个
      for (let i = 0; i < this.data.categories.length; i++) {
        const rect = res[i];
        if (!rect) continue;
        
        // 只要标签的底部 低于 容器顶部（标签已经进入屏幕）
        if (rect.bottom < listRect.top + 100) {
          curId = this.data.categories[i].id;
        }
      }
  
      if (curId !== this.data.activeCategory) {
        this.setData({ activeCategory: curId });
      }
    })
  },

  // 数据已在 getOrderData 中全量加载，按分类切换无需额外请求
  loadCategoryGoods() {},
  loadAllCategories() {},

  // 后台刷新所有菜品库存（不显示loading）
  refreshStock() {
    api.order.getFullMenu()
      .then(data => {
        const inner = data.data || data;
        const menuData = inner.data || inner;
        let rawCats = [];
        let rawGoodsList = {};
        if (Array.isArray(menuData)) {
          rawCats = menuData;
        } else if (Array.isArray(menuData.categories)) {
          rawCats = menuData.categories;
          rawGoodsList = menuData.goodsList || {};
        } else if (Array.isArray(menuData.list)) {
          rawCats = menuData.list;
          rawGoodsList = menuData.goodsList || {};
        } else if (Array.isArray(menuData.data)) {
          rawCats = menuData.data;
        }
        const goodsList = {};
        const allItems = [];
        const goodsKeys = Object.keys(rawGoodsList);
        if (goodsKeys.length > 0) {
          goodsKeys.forEach(catId => {
            const items = this.addImageUrlsToGoods(rawGoodsList[catId] || []);
            goodsList[catId] = items;
            allItems.push(...items);
          });
        } else {
          rawCats.forEach(cat => {
            const catId = String(cat.id);
            const itemList = cat.items || cat.dishes || cat.products || cat.goods || cat.list || cat.foods || [];
            const items = this.addImageUrlsToGoods(itemList);
            goodsList[catId] = items;
            allItems.push(...items);
          });
        }
        goodsList['all'] = allItems;
        this.setData({ goodsList });
        this.updateMergedGoodsList();
      })
      .catch(() => {});
  },

  updateMergedGoodsList() {
    const { categories, goodsList } = this.data;
    const merged = [];
    let index = 0;
    categories.forEach(c => {
      merged.push({ type: 'category', id: c.id, name: c.name, uniqueKey: `cat-${c.id}-${index}` });
      index++;
      (goodsList[c.id] || []).forEach((g) => {
        merged.push({ ...g, uniqueKey: `item-${index}` });
        index++;
      });
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
      newCart[key].count = newCart[key].quantity;
    } else {
      if (goods.stock <= 0) return wx.showToast({ title: '库存不足', icon: 'none' });
      newCart[key] = { ...goods, quantity: 1, count: 1 };
    }
    this.syncCartState(newCart);
  },

  increaseQuantity(e) {
    const id = e.currentTarget.dataset.id + '';
    const newCart = { ...this.data.cart };
    if (!newCart[id]) return;
    if (newCart[id].quantity >= newCart[id].stock) return wx.showToast({ title: '库存不足', icon: 'none' });
    newCart[id].quantity++;
    newCart[id].count = newCart[id].quantity;
    this.syncCartState(newCart);
  },

  decreaseQuantity(e) {
    const id = e.currentTarget.dataset.id + '';
    const newCart = { ...this.data.cart };
    if (!newCart[id]) return;
    if (newCart[id].quantity <= 1) delete newCart[id];
    else {
      newCart[id].quantity--;
      newCart[id].count = newCart[id].quantity;
    }
    this.syncCartState(newCart);
  },

  onQuantityInput(e) {
    const id = e.currentTarget.dataset.id + '';
    const val = Math.max(0, parseInt(e.detail.value) || 0);
    const goods = this.data.mergedGoodsList.find(i => i.id == id);
    if (!goods) return;
    const newCart = { ...this.data.cart };
    const finalQty = Math.min(val, goods.stock);
    if (finalQty === 0) delete newCart[id];
    else {
      newCart[id] = { ...newCart[id], quantity: finalQty, count: finalQty };
    }
    this.syncCartState(newCart);
  },

  syncCartState(newCart) {
    let count = 0, total = 0;
    const cartWithChecked = {};
    for (const key in newCart) {
      const item = newCart[key];
      const itemQuantity = item.quantity || item.count || 0;
      cartWithChecked[key] = {
        ...item,
        id: String(item.id || key),
        quantity: itemQuantity,
        count: itemQuantity,
        checked: true
      };
      count += itemQuantity;
      total += (item.price || 0) * itemQuantity;
    }
    this.setData({ cart: cartWithChecked, cartItems: Object.values(cartWithChecked), cartCount: count, totalPrice: +total.toFixed(2) });
    wx.setStorageSync('orderCart', cartWithChecked);
  },

  restoreCart(cart) {
    let count = 0, total = 0;
    const restoredCart = {};
    Object.entries(cart || {}).forEach(([key, item]) => {
      const itemQuantity = item.quantity || item.count || 0;
      if (itemQuantity <= 0) return;
      const itemId = String(item.id || key);
      restoredCart[itemId] = {
        ...item,
        id: itemId,
        image: this.processImageUrl(item.image || ''),
        quantity: itemQuantity,
        count: itemQuantity,
        checked: item.checked !== false
      };
      count += itemQuantity;
      total += (item.price || 0) * itemQuantity;
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
    
    // 自动勾选所有点餐商品，确保结算时能看到所有商品
    const updatedCart = {};
    for (const key in this.data.cart) {
      updatedCart[key] = {
        ...this.data.cart[key],
        checked: true
      };
    }
    
    // 更新本地数据和存储
    this.setData({ cart: updatedCart, cartItems: Object.values(updatedCart) });
    wx.setStorageSync('orderCart', updatedCart);
    
    wx.navigateTo({
      url: `/user-pages/confirm-order/confirm-order?type=food&tableNumber=${this.data.tableNumber}`
    });
  },

  navigateToGoodsDetail(e) {
    const id = e.currentTarget.dataset.id;
    // 把该菜品数据传给详情页，避免详情页再调错误的 /api/goods/{id}
    const goods = this.data.mergedGoodsList.find(i => String(i.id) === String(id));
    if (goods) {
      getApp().globalData.foodDetail = goods;
    }
    wx.navigateTo({
      url: `/user-pages/order-foods-detail/order-foods-detail?id=${id}`
    });
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
    wx.showToast({ title: `已选择${id}号桌`, icon: 'success' });
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
    api.table.getList()
      .then(data => {
        this.setData({ tableList: (data || []).map(t => ({ id: String(t.id), name: t.name })) });
      })
      .catch(() => {
        this.setData({ tableList: [] });
      });
  },

  stopPropagation() { return false },
  navigateToService() {
    wx.showModal({ title: '客服', content: '电话：15876534944\n微信：njjtnc15876534944', showCancel: false });
  }
});