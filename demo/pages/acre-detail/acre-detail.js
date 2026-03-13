Page({
  data: {
    acreDetail: {}
  },
  
  onLoad: function(options) {
    const id = options.id;
    this.loadAcreDetail(id);
  },
  
  loadAcreDetail: function(id) {
    wx.showLoading({
      title: '加载中...',
    });
    
    wx.request({
      url: 'http://localhost:5162/api/DemoApi/acres/' + id,
      method: 'GET',
      success: (res) => {
        wx.hideLoading();
        if (res.data.code === 0) {
          this.setData({
            acreDetail: res.data.data
          });
        } else {
          wx.showToast({
            title: '加载失败',
            icon: 'none'
          });
        }
      },
      fail: (err) => {
        wx.hideLoading();
        wx.showToast({
          title: '网络错误',
          icon: 'none'
        });
      }
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