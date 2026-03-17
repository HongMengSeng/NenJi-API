Page({
  data: {
    // 支付金额
    totalPrice: 0,
    // 加载状态
    loading: false,
    // 支付状态
    payStatus: 'pending' // pending, success, failed
  },

  onLoad: function (options) {
    // 获取支付金额
    const totalPrice = options.totalPrice || 0;
    this.setData({
      totalPrice: totalPrice
    });
    
    // 自动开始支付
    this.startPayment();
  },

  // 开始支付
  startPayment: function() {
    this.setData({ loading: true });
    
    // 模拟支付过程
    setTimeout(() => {
      this.setData({ 
        loading: false,
        payStatus: 'success' 
      });
    }, 2000);
  },

  // 重新支付
  retryPayment: function() {
    this.setData({ payStatus: 'pending' });
    this.startPayment();
  },

  // 查看订单
  viewOrder: function() {
    wx.switchTab({
      url: '/pages/order/order'
    });
  }
});