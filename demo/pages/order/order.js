const api = require('../../utils/api');

Page({
  data: {
    // 当前激活的分类
    activeCategory: 'vegetables',
    // 分类列表
    categories: [],
    // 商品列表
    goodsList: {},
    // 购物车
    cart: {},
    // 购物车商品数组（用于显示）
    cartItems: [],
    // 购物车商品数量
    cartCount: 0,
    // 总价格
    totalPrice: 0,
    // 加载状态
    loading: true,
    // 懒加载状态
    lazyLoading: false,
    // 购物车弹窗显示状态
    showCartModal: false,
    // 购物车数据是否已缓存
    cartCached: false,
    // 防抖计时器
    debounceTimer: null,
    // 待更新的商品数量
    pendingUpdates: {},
    // 防止重复点击
    isProcessing: false
  },

  onLoad: function (options) {
    // 页面加载时获取点餐数据
    this.getOrderData();
  },

  // 获取点餐数据
  getOrderData: function() {
    wx.showLoading({ title: '加载中...', mask: true });

    api.request({
      url: 'http://192.168.203.56/api/order/getOrderData',
      method: 'GET'
    })
      .then(data => {
        // 解析API返回的数据结构
        const apiData = data.data.data;
        const categories = apiData.categories || [];
        const goodsList = apiData.goodsList || {};
        const currentCategory = categories.length > 0 ? categories[0].id : '';

        this.setData({
          categories: categories,
          goodsList: goodsList,
          activeCategory: currentCategory,
          loading: false
        });
      })
      .catch(err => {
        this.setData({ loading: false });
        wx.showToast({
          title: err.message || '点餐数据加载失败',
          icon: 'none'
        });
      })
      .finally(() => {
        wx.hideLoading();
      });
  },

  // 切换分类
  switchCategory: function(e) {
    const category = e.currentTarget.dataset.category;
    this.setData({
      activeCategory: category
    });
  },

  // 添加商品到购物车
  addToCart: function(e) {
    // 防止重复点击
    if (this.data.isProcessing) return;
    this.setData({ isProcessing: true });
    
    const { category, index } = e.currentTarget.dataset;
    const goods = this.data.goodsList[category][index];
    
    // 检查库存
    if (goods.stock <= 0) {
      wx.showToast({
        title: '商品已售罄',
        icon: 'none'
      });
      this.setData({ isProcessing: false });
      return;
    }
    
    // 检查购物车中是否已有该商品，如果有，检查数量是否超过库存
    const existingQuantity = this.data.cart[goods.id] ? this.data.cart[goods.id].quantity : 0;
    if (existingQuantity + 1 > goods.stock) {
      wx.showToast({
        title: '库存不足',
        icon: 'none'
      });
      this.setData({ isProcessing: false });
      return;
    }
    
    // 更新购物车
    const newCart = JSON.parse(JSON.stringify(this.data.cart));
    if (newCart[goods.id]) {
      newCart[goods.id].quantity += 1;
    } else {
      newCart[goods.id] = {
        ...goods,
        quantity: 1
      };
    }
    
    // 计算购物车数量和总价格
    let cartCount = 0;
    let totalPrice = 0;
    for (const item in newCart) {
      cartCount += newCart[item].quantity;
      totalPrice += newCart[item].price * newCart[item].quantity;
    }
    // 保留两位小数
    totalPrice = parseFloat(totalPrice.toFixed(2));
    
    // 将购物车对象转换为数组（用于显示）
    const cartItems = Object.values(newCart);
    
    // 更新待更新列表
    const newPendingUpdates = JSON.parse(JSON.stringify(this.data.pendingUpdates));
    newPendingUpdates[goods.id] = (newPendingUpdates[goods.id] || 0) + 1;
    
    // 批量更新数据，减少setData调用
    this.setData({
      cart: newCart,
      cartItems: cartItems,
      cartCount: cartCount,
      totalPrice: totalPrice,
      pendingUpdates: newPendingUpdates,
      isProcessing: false
    });
    
    // 缓存购物车数据
    wx.setStorageSync('orderCart', newCart);
    
    // 触发防抖更新
    this.debounceUpdate();
  },

  // 查看购物车详情
  viewCart: function() {
    if (this.data.cartCount === 0) {
      wx.showToast({
        title: '购物车为空',
        icon: 'none'
      });
      return;
    }
    
    // 确保cartItems是最新的
    const cartItems = Object.values(this.data.cart);
    
    // 首次打开购物车时缓存数据
    if (!this.data.cartCached) {
      wx.setStorageSync('orderCart', this.data.cart);
      this.setData({ cartCached: true });
    }
    
    // 显示购物车详情弹窗
    this.setData({
      cartItems: cartItems,
      showCartModal: true
    });
  },

  // 防抖函数
  debounceUpdate: function() {
    if (this.data.debounceTimer) {
      clearTimeout(this.data.debounceTimer);
    }
    
    this.setData({
      debounceTimer: setTimeout(() => {
        // 调用API更新商品数量
        this.updateGoodsQuantity();
        // 清空待更新列表
        this.setData({ pendingUpdates: {} });
      }, 500)
    });
  },

  // 更新商品数量的API调用
  updateGoodsQuantity: function() {
    const updates = this.data.pendingUpdates;
    if (Object.keys(updates).length === 0) return;
    
    // 调用真实API更新商品数量
    console.log('调用API更新商品数量:', updates);
    
    // 这里可以添加真实的API调用
    // api.request({
    //   url: '/api/order/updateGoodsQuantity',
    //   method: 'POST',
    //   data: { updates }
    // })
    //   .then(res => {
    //     console.log('商品数量更新成功:', res);
    //   })
    //   .catch(err => {
    //     console.error('商品数量更新失败:', err);
    //   });
  },

  // 隐藏购物车详情弹窗
  hideCartModal: function() {
    this.setData({
      showCartModal: false
    });
  },

  // 增加商品数量
  increaseQuantity: function(e) {
    // 防止重复点击
    if (this.data.isProcessing) return;
    this.setData({ isProcessing: true });
    
    const goodsId = e.currentTarget.dataset.id;
    const cart = this.data.cart;
    const goods = cart[goodsId];
    
    // 检查库存
    let stockAvailable = false;
    let categoryFound = '';
    let indexFound = -1;
    let currentStock = 0;
    
    for (const category in this.data.goodsList) {
      const categoryGoods = this.data.goodsList[category];
      for (let i = 0; i < categoryGoods.length; i++) {
        if (categoryGoods[i].id === goodsId) {
          currentStock = categoryGoods[i].stock;
          // 检查库存是否足够（购物车数量+1后不超过库存）
          if (currentStock > 0 && (goods.quantity + 1) <= currentStock) {
            stockAvailable = true;
          }
          categoryFound = category;
          indexFound = i;
          break;
        }
      }
      if (stockAvailable) break;
    }
    
    if (!stockAvailable) {
      wx.showToast({
        title: currentStock > 0 ? '库存不足' : '商品已售罄',
        icon: 'none'
      });
      this.setData({ isProcessing: false });
      return;
    }
    
    // 更新购物车
    const newCart = JSON.parse(JSON.stringify(this.data.cart));
    newCart[goodsId].quantity += 1;
    
    // 计算购物车数量和总价格
    let cartCount = 0;
    let totalPrice = 0;
    for (const item in newCart) {
      cartCount += newCart[item].quantity;
      totalPrice += newCart[item].price * newCart[item].quantity;
    }
    // 保留两位小数
    totalPrice = parseFloat(totalPrice.toFixed(2));
    
    // 将购物车对象转换为数组（用于显示）
    const cartItems = Object.values(newCart);
    
    // 更新待更新列表
    const newPendingUpdates = JSON.parse(JSON.stringify(this.data.pendingUpdates));
    newPendingUpdates[goodsId] = (newPendingUpdates[goodsId] || 0) + 1;
    
    // 批量更新数据，减少setData调用
    this.setData({
      cart: newCart,
      cartItems: cartItems,
      cartCount: cartCount,
      totalPrice: totalPrice,
      pendingUpdates: newPendingUpdates,
      isProcessing: false
    });
    
    // 缓存购物车数据
    wx.setStorageSync('orderCart', newCart);
    
    // 触发防抖更新
    this.debounceUpdate();
  },

  // 减少商品数量
  decreaseQuantity: function(e) {
    // 防止重复点击
    if (this.data.isProcessing) return;
    this.setData({ isProcessing: true });
    
    let goodsId;
    let goods;
    
    // 检查是从商品列表还是购物车弹窗调用
    if (e.currentTarget.dataset.category && e.currentTarget.dataset.index !== undefined) {
      // 从商品列表调用
      const { category, index } = e.currentTarget.dataset;
      goods = this.data.goodsList[category][index];
      goodsId = goods.id;
    } else {
      // 从购物车弹窗调用
      goodsId = e.currentTarget.dataset.id;
      goods = this.data.cart[goodsId];
    }
    
    // 检查商品是否在购物车中
    if (!this.data.cart[goodsId]) {
      this.setData({ isProcessing: false });
      return;
    }
    
    // 更新购物车
    const newCart = JSON.parse(JSON.stringify(this.data.cart));
    if (newCart[goodsId].quantity <= 1) {
      // 从购物车中移除
      delete newCart[goodsId];
    } else {
      // 减少数量
      newCart[goodsId].quantity -= 1;
    }
    
    // 计算购物车数量和总价格
    let cartCount = 0;
    let totalPrice = 0;
    for (const item in newCart) {
      cartCount += newCart[item].quantity;
      totalPrice += newCart[item].price * newCart[item].quantity;
    }
    // 保留两位小数
    totalPrice = parseFloat(totalPrice.toFixed(2));
    
    // 将购物车对象转换为数组（用于显示）
    const cartItems = Object.values(newCart);
    
    // 更新待更新列表
    const newPendingUpdates = JSON.parse(JSON.stringify(this.data.pendingUpdates));
    newPendingUpdates[goodsId] = (newPendingUpdates[goodsId] || 0) - 1;
    
    // 批量更新数据，减少setData调用
    this.setData({
      cart: newCart,
      cartItems: cartItems,
      cartCount: cartCount,
      totalPrice: totalPrice,
      pendingUpdates: newPendingUpdates,
      isProcessing: false
    });
    
    // 缓存购物车数据
    wx.setStorageSync('orderCart', newCart);
    
    // 触发防抖更新
    this.debounceUpdate();
  },

  // 结算
  checkout: function() {
    if (this.data.cartCount === 0) {
      wx.showToast({
        title: '购物车为空',
        icon: 'none'
      });
      return;
    }
    
    // 跳转到确认订单页面
    wx.navigateTo({
      url: '/pages/confirm-order/confirm-order'
    });
  },

  // 跳转到商品详情页面
  navigateToGoodsDetail: function(e) {
    const goodsId = e.currentTarget.dataset.id;
    wx.navigateTo({
      url: `/pages/goods-detail/goods-detail?id=${goodsId}`
    });
  },

  // 页面滚动到底部触发懒加载
  onReachBottom: function() {
    // 防止重复加载
    if (this.data.lazyLoading) return;
    
    // 模拟懒加载
    this.setData({ lazyLoading: true });
    
    // 模拟加载过程
    setTimeout(() => {
      // 这里可以添加真实的加载逻辑，比如加载更多商品
      this.setData({ lazyLoading: false });
    }, 1000);
  }
});