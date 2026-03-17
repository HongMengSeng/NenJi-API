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
    wx.showLoading({ title: '加载中...' });
    
    // // 使用虚拟API数据
    // setTimeout(() => {
    //   const mockData = {
    //     categories: [
    //       { id: 'vegetables', name: '新鲜蔬菜' },
    //       { id: 'meat', name: '肉类产品' },
    //       { id: 'eggs', name: '禽蛋产品' },
    //       { id: 'dairy', name: '乳制品' },
    //       { id: 'staple', name: '主食' }
    //     ],
    //     goodsList: {
    //       vegetables: [
    //         {
    //           id: 1,
    //           name: '有机生菜',
    //           image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20organic%20lettuce&image_size=square',
    //           price: 30,
    //           sold: 150,
    //           stock: 30
    //         },
    //         {
    //           id: 2,
    //           name: '农家西红柿',
    //           image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20tomatoes&image_size=square',
    //           price: 30,
    //           sold: 200,
    //           stock: 30
    //         },
    //         {
    //           id: 3,
    //           name: '新鲜黄瓜',
    //           image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20cucumbers&image_size=square',
    //           price: 30,
    //           sold: 180,
    //           stock: 30
    //         }
    //       ],
    //       meat: [
    //         {
    //           id: 4,
    //           name: '土猪肉',
    //           image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20pork%20meat&image_size=square',
    //           price: 30,
    //           sold: 100,
    //           stock: 30
    //         },
    //         {
    //           id: 5,
    //           name: '农家土鸡',
    //           image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20chicken&image_size=square',
    //           price: 30,
    //           sold: 80,
    //           stock: 30
    //         }
    //       ],
    //       eggs: [
    //         {
    //           id: 6,
    //           name: '土鸡蛋',
    //           image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20eggs&image_size=square',
    //           price: 30,
    //           sold: 300,
    //           stock: 30
    //         },
    //         {
    //           id: 7,
    //           name: '鸭蛋',
    //           image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20duck%20eggs&image_size=square',
    //           price: 30,
    //           sold: 150,
    //           stock: 30
    //         },
    //         {
    //           id: 8,
    //           name: '鹅蛋',
    //           image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20goose%20eggs&image_size=square',
    //           price: 30,
    //           sold: 50,
    //           stock: 30
    //         }
    //       ],
    //       dairy: [
    //         {
    //           id: 9,
    //           name: '新鲜牛奶',
    //           image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20milk&image_size=square',
    //           price: 30,
    //           sold: 200,
    //           stock: 30
    //         },
    //         {
    //           id: 10,
    //           name: '农家酸奶',
    //           image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=homemade%20yogurt&image_size=square',
    //           price: 30,
    //           sold: 180,
    //           stock: 30
    //         }
    //       ],
    //       staple: [
    //         {
    //           id: 11,
    //           name: '农家大米',
    //           image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20rice&image_size=square',
    //           price: 30,
    //           sold: 250,
    //           stock: 30
    //         },
    //         {
    //           id: 12,
    //           name: '手工面条',
    //           image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=homemade%20noodles&image_size=square',
    //           price: 30,
    //           sold: 150,
    //           stock: 30
    //         }
    //       ]
    //     }
    //   };
      
    //   // 使用虚拟数据
    //   this.setData({
    //     categories: mockData.categories,
    //     goodsList: mockData.goodsList,
    //     loading: false
    //   });
      
    //   wx.hideLoading();
    // }, 500);
    
    // 真实API调用（已注释，需要时取消注释）

    api.request({
      url: 'http://localhost:5162/api/order/getOrderData',
      method: 'GET'
    }).then(res => {
      // 核心修复：真实API数据在res.data.data中，先校验数据合法性
      if (!res || !res.data || !res.data.data) {
        throw new Error('接口返回数据格式异常，缺少data字段');
      }

      const { categories, goodsList } = res.data.data;
      
      // 校验分类和商品数据是否存在
      if (!Array.isArray(categories) || Object.keys(goodsList).length === 0) {
        throw new Error('分类或商品列表数据为空');
      }
      
      // 更新数据
      this.setData({
        categories: categories,
        goodsList: goodsList,
        loading: false
      });
      
      wx.hideLoading();
    }).catch(err => {
      console.error('获取点餐数据失败:', err);
      this.setData({ loading: false });
      wx.hideLoading();
      wx.showToast({ 
        title: '数据加载失败，请重试', 
        icon: 'none',
        duration: 2000
      });
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
    
    // 虚拟API：只打印日志，不实际调用API
    console.log('待更新的商品数量:', updates);
    
    // 真实API调用（已注释，需要时取消注释）
    /*
    wx.request({
      url: 'http://localhost:5162/api/order/updateGoodsQuantity',
      method: 'POST',
      data: { updates },
      success: (res) => {
        console.log('商品数量更新成功:', res.data);
      },
      fail: (err) => {
        console.error('商品数量更新失败:', err);
        // 错误处理：不影响前端显示
      }
    });
    */
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
    
    const goodsId = e.currentTarget.dataset.id;
    const cart = this.data.cart;
    const goods = cart[goodsId];
    
    // 更新购物车
    const newCart = JSON.parse(JSON.stringify(this.data.cart));
    if (goods.quantity <= 1) {
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