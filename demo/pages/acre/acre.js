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
    const api = require('../../utils/api');
    
    wx.showLoading({
      title: '加载中...',
    });
    
    api.request({
      url: 'http://127.0.0.1:5000/api/acres/index',
      method: 'GET'
    })
    .then(res => {
      wx.hideLoading();
      this.setData({
        acreList: res.list,
        swiperList: res.swiperList
      });
    })
    .catch(err => {
      wx.hideLoading();
    });
  },
  
  navigateToDetail: function(e) {
    const id = e.currentTarget.dataset.id;
    wx.navigateTo({
      url: '/pages/acre-detail/acre-detail?id=' + id
    });
  }
});