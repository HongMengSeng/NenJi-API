Page({
  data: {
    acreList: [],
    swiperList: []
  },
  
  onLoad: function() {
    // 从后台加载数据
    this.loadAcreData();
  },
  
  loadAcreData: function() {
    wx.showLoading({
      title: '加载中...',
    });
    
    wx.request({
      url: 'http://localhost:5162/api/DemoApi/acres',
      method: 'GET',
      success: (res) => {
        wx.hideLoading();
        if (res.data.code === 0) {
          this.setData({
            acreList: res.data.data.list,
            swiperList: res.data.data.swiperList
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
  
  navigateToDetail: function(e) {
    const id = e.currentTarget.dataset.id;
    wx.navigateTo({
      url: '/pages/acre-detail/acre-detail?id=' + id
    });
  }
});