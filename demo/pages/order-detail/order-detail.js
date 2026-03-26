const api = require('../../utils/api');

Page({
  data: {
    order: {
      id: '',
      status: '',
      statusText: '',
      createTime: '',
      payTime: null,
      shippingTime: null,
      completeTime: null,
      totalPrice: 0,
      shippingAddress: {
        name: '',
        phone: '',
        address: ''
      },
      items: [],
      paymentMethod: null,
      transactionId: null
    },
    loading: true
  },

  onLoad(options) {
    const orderId = options.id;

    if (!orderId) {
      this.setData({ loading: false });
      wx.showToast({
        title: '缺少订单ID',
        icon: 'none'
      });
      return;
    }

    this.getOrderDetail(orderId);
  },

  getOrderDetail(orderId) {
    wx.showLoading({ title: '加载中...' });

    api.request({
      url: `/api/OrderDetails/${orderId}`,
      method: 'GET'
    })
      .then((data) => {
        this.setData({
<<<<<<< HEAD
          dish: {
            id: data.id || dishId,
            name: data.name || '',
            price: Number(data.price || 0),
            image: data.image || '',
            detailImage: data.detailImage || data.image || '',
            description: data.description || '',
            ingredients: data.ingredients || '',
            cookingTime: data.cookingTime || '',
            stock: data.stock || 0
=======
          order: data.order || {
            id: orderId,
            status: '',
            statusText: '',
            createTime: '',
            payTime: null,
            shippingTime: null,
            completeTime: null,
            totalPrice: 0,
            shippingAddress: {
              name: '',
              phone: '',
              address: ''
            },
            items: [],
            paymentMethod: null,
            transactionId: null
>>>>>>> 69fb92a4f1e5da4e997a2ae2b3fa15b390dec831
          },
          loading: false
        });
      })
      .catch((err) => {
        console.error('获取订单详情失败:', err);
        this.setData({ loading: false });
<<<<<<< HEAD
=======
        wx.showToast({
          title: '获取订单详情失败',
          icon: 'none'
        });
>>>>>>> 69fb92a4f1e5da4e997a2ae2b3fa15b390dec831
      })
      .finally(() => {
        wx.hideLoading();
      });
<<<<<<< HEAD
  },

  addToCart() {
    // 检查桌台号码
    const tableNumber = wx.getStorageSync('tableNumber');
    if (!tableNumber) {
      wx.showToast({
        title: '请先选择桌台号码',
        icon: 'none'
      });
      return;
    }

    const dish = this.data.dish;
    const currentCart = wx.getStorageSync('orderCart') || {};
    const newCart = { ...currentCart };
    const key = String(dish.id);

    // 检查库存
    const currentQuantity = newCart[key] ? newCart[key].quantity : 0;
    const stock = dish.stock || 0;
    if (currentQuantity >= stock) {
      wx.showToast({ title: '库存不足', icon: 'none' });
      return;
    }

    if (newCart[key]) {
      newCart[key].quantity += 1;
    } else {
      newCart[key] = { 
        ...dish, 
        quantity: 1 
      };
    }

    wx.setStorageSync('orderCart', newCart);
    this.updateCartCount();
    wx.showToast({
      title: '已加入购物车',
      icon: 'success'
    });
=======
>>>>>>> 69fb92a4f1e5da4e997a2ae2b3fa15b390dec831
  },

  payOrder() {
    const orderId = this.data.order.id;
    wx.navigateTo({
      url: `/pages/pay/pay?orderId=${orderId}`
    });
  },

  goToOrders() {
    wx.navigateTo({
      url: '/pages/orders/orders'
    });
  },

  goBack() {
    wx.navigateBack();
  },

  // 取消订单
  cancelOrder() {
    wx.showModal({
      title: '确认取消',
      content: '确定要取消这个订单吗？',
      success: (res) => {
        if (res.confirm) {
          wx.showLoading({ title: '取消中...' });
          
          api.request({
            url: `/api/OrderDetails/${this.data.order.id}/cancel`,
            method: 'POST',
            data: {
              reason: '用户主动取消'
            }
          })
          .then((data) => {
            wx.hideLoading();
            wx.showToast({ title: '订单已取消', icon: 'success' });
            // 刷新订单详情
            this.getOrderDetail(this.data.order.id);
          })
          .catch((err) => {
            console.error('取消订单失败:', err);
            wx.hideLoading();
            wx.showToast({ title: '取消订单失败', icon: 'none' });
          });
        }
      }
    });
  },

<<<<<<< HEAD
  updateCartCount() {
    const cart = wx.getStorageSync('orderCart') || {};
    let totalCount = 0;
    Object.values(cart).forEach(item => {
      totalCount += item.quantity || 0;
    });
    this.setData({ cartCount: totalCount });
  },

  goToCart() {
    wx.navigateBack({
      delta: 1
=======
  // 确认收货
  confirmReceipt() {
    wx.showModal({
      title: '确认收货',
      content: '确定已收到商品吗？',
      success: (res) => {
        if (res.confirm) {
          wx.showLoading({ title: '确认中...' });
          
          api.request({
            url: `/api/OrderDetails/${this.data.order.id}/confirm`,
            method: 'POST'
          })
          .then((data) => {
            wx.hideLoading();
            wx.showToast({ title: '收货成功', icon: 'success' });
            // 刷新订单详情
            this.getOrderDetail(this.data.order.id);
          })
          .catch((err) => {
            console.error('确认收货失败:', err);
            wx.hideLoading();
            wx.showToast({ title: '确认收货失败', icon: 'none' });
          });
        }
      }
>>>>>>> 69fb92a4f1e5da4e997a2ae2b3fa15b390dec831
    });
  }
});
