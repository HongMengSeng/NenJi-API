const api = require('../../utils/api');

Page({
  data: {
    goods: {},
    loading: true,
    cartCount: 0,
    cart: {}
  },

  onLoad(options) {
    const goodsId = options.id;
    if (goodsId) {
      this.getGoodsDetail(goodsId);
    }
    this.updateCartCount();
  },

  onShow() {
    this.updateCartCount();
  },

  getGoodsDetail(goodsId) {
    wx.showLoading({ title: '加载中...' });
    
    api.request({
      url: '/api/goods/detail',
      method: 'GET',
      data: { id: goodsId }
    }).then(data => {
      this.setData({
        goods: data,
        loading: false
      });
    }).catch(err => {
      console.error('加载商品详情失败', err);
      this.setData({ loading: false });
      wx.showToast({ title: '加载失败', icon: 'none' });
    }).finally(() => wx.hideLoading());
  },

  updateCartCount() {
    try {
      const cart = wx.getStorageSync('orderCart') || {};
      let count = 0;
      Object.values(cart).forEach(item => {
        count += item.quantity || 0;
      });
      this.setData({ 
        cart: cart,
        cartCount: count 
      });
    } catch (e) {
      console.error('更新购物车数量失败', e);
    }
  },

  addToCart() {
    const goods = this.data.goods;
    if (!goods || !goods.id) {
      wx.showToast({ title: '商品信息加载失败', icon: 'none' });
      return;
    }

    try {
      const cart = wx.getStorageSync('orderCart') || {};
      const key = String(goods.id);
      
      if (cart[key]) {
        if (cart[key].quantity >= goods.stock) {
          wx.showToast({ title: '库存不足', icon: 'none' });
          return;
        }
        cart[key].quantity += 1;
      } else {
        cart[key] = { ...goods, quantity: 1 };
      }
      
      wx.setStorageSync('orderCart', cart);
      this.updateCartCount();
      wx.showToast({ title: '加入购物车成功', icon: 'success' });
    } catch (e) {
      console.error('加入购物车失败', e);
      wx.showToast({ title: '加入购物车失败', icon: 'none' });
    }
  },

  buyNow() {
    const goods = this.data.goods;
    if (!goods || !goods.id) {
      wx.showToast({ title: '商品信息加载失败', icon: 'none' });
      return;
    }

    // 先加入购物车
    try {
      const cart = wx.getStorageSync('orderCart') || {};
      const key = String(goods.id);
      cart[key] = { ...goods, quantity: 1 };
      wx.setStorageSync('orderCart', cart);
      
      // 跳转到确认订单页面
      wx.navigateTo({ url: '/pages/confirm-order/confirm-order' });
    } catch (e) {
      console.error('立即购买失败', e);
      wx.showToast({ title: '操作失败', icon: 'none' });
    }
  },

  viewCart() {
    wx.navigateTo({ url: '/pages/cart/cart' });
  }
});