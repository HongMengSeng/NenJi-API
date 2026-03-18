Page({
  data: {
    acreDetail: {}
  },
  
  onLoad: function(options) {
    const id = options.id;
    this.loadAcreDetail(id);
  },
  
  loadAcreDetail: function(id) {
    const api = require('../../utils/api');
    
    wx.showLoading({
      title: '加载中...',
    });
    
    api.request({
      url: '/api/DemoApi/acres/' + id,
      method: 'GET'
    })
    .then(res => {
      wx.hideLoading();
      this.setData({
        acreDetail: res
      });
    })
    .catch(err => {
      wx.hideLoading();
    });
  },
  
  confirmPurchase: function() {
    wx.showModal({
      title: '确认购买',
      content: '您确定要购买此地块吗？',
      success: function(res) {
        if (res.confirm) {
          wx.showToast({
            title: '购买成功！',
            icon: 'success',
            duration: 2000
          });
          // 这里可以添加跳转到支付页面的逻辑
        }
      }
    });
  }
});