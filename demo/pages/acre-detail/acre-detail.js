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
      // 确保数据结构完整
      const acreDetail = {
        ...res,
        videoUrl: res.videoUrl || 'https://example.com/farm-video.mp4', // 视频URL
        remainingAcres: res.remainingAcres || 50, // 剩余亩数
        soldAcres: res.soldAcres || 150, // 已售亩数
        longExampleImage: res.longExampleImage || '/images/forest.png' // 农场示例图片
      };
      this.setData({
        acreDetail: acreDetail
      });
    })
    .catch(err => {
      wx.hideLoading();
      // 模拟数据，确保页面正常显示
      this.setData({
        acreDetail: {
          name: '有机农场认购',
          price: '¥1980/亩',
          description: '认购一亩有机农场，体验种植乐趣，收获绿色健康农产品。',
          videoUrl: 'https://example.com/farm-video.mp4',
          remainingAcres: 50,
          soldAcres: 150,
          swiperList: [
            { id: 1, image: '/images/forest.png' },
            { id: 2, image: '/images/we.png' }
          ],
          longExampleImage: '/images/forest.png' // 4张图拼成的长图
        }
      });
    });
  },
  
  contactService: function() {
    wx.showModal({
      title: '能记家庭农场客服',
      content: '手机号：15876534944\n     微信号：njjtnc15876534944',
      showCancel: false
    });
  },
  
  confirmPurchase: function() {
    const remainingAcres = this.data.acreDetail.remainingAcres || 0;
    
    // 检查是否已卖完
    if (remainingAcres <= 0) {
      wx.showToast({
        title: '已卖完',
        icon: 'none'
      });
      return;
    }
    
    wx.showModal({
      title: '选择购买亩数',
      content: `当前剩余 ${remainingAcres} 亩`,
      editable: true,
      placeholderText: '请输入亩数',
      success: function(res) {
        if (res.confirm) {
          if (!res.content || res.content.trim() === '') {
            wx.showToast({
              title: '请输入购买亩数',
              icon: 'none'
            });
            return;
          }
          const acres = parseInt(res.content);
          if (acres > 0) {
            if (acres > remainingAcres) {
              wx.showToast({
                title: `购买数量不能超过剩余的 ${remainingAcres} 亩`,
                icon: 'none'
              });
              return;
            }
            // 计算总价格
            const price = parseFloat(this.data.acreDetail.price.replace('¥', ''));
            const totalPrice = price * acres;
            
            // 跳转到支付页面
            wx.navigateTo({
              url: '/pages/pay/pay?totalPrice=' + totalPrice
            });
          } else {
            wx.showToast({
              title: '请输入有效的亩数',
              icon: 'none'
            });
          }
        }
      }.bind(this)
    });
  }
});