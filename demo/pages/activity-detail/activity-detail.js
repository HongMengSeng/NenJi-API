const api = require('../../utils/api');

Page({
  data: {
    activity: {},
    loading: true
  },

  onLoad: function(options) {
    const activityId = options.id;
    if (activityId) {
      this.getActivityDetail(activityId);
    }
  },

  getActivityDetail: function(activityId) {
    wx.showLoading({ title: '加载中...', mask: true });

    api.request({
      url: '/api/activity/detail',
      method: 'GET',
      data: {
        id: activityId
      }
    })
      .then(data => {
        // 确保活动有图片，如果没有image字段但有images数组，使用第一张图片
        if (data && !data.image && data.images && data.images.length > 0) {
          data.image = data.images[0];
        }
        this.setData({
          activity: data || {},
          loading: false
        });
      })
      .catch(err => {
        wx.showToast({
          title: err.message || '活动详情加载失败',
          icon: 'none'
        });
        this.setData({ loading: false });
      })
      .finally(() => {
        wx.hideLoading();
      });
  },

  contactService: function() {
    wx.showModal({
      title: '能记家庭农场客服',
      content: '手机号：15876534944\n微信号：njjtnc15876534944',
      showCancel: false
    });
  },

  registerActivity: function() {
    const activity = this.data.activity;
    const maxPeople = parseInt(activity.people) || 0;
    
    // 检查是否已卖完
    if (maxPeople <= 0) {
      wx.showToast({
        title: '活动已卖完',
        icon: 'none'
      });
      return;
    }
    
    const ticketPrice = 198; // 卷的价格
    
    // 直接显示数量输入弹窗
    wx.showModal({
      title: `你要买几张卷？（还剩${maxPeople}张）`,

      editable: true,
      placeholderText: '请输入数量',
      success: (res) => {
        if (res.confirm) {
          if (!res.content || res.content.trim() === '') {
            wx.showToast({
              title: '请输入购买数量',
              icon: 'none'
            });
            return;
          }
          
          const quantity = parseInt(res.content);
          if (quantity <= 0) {
            wx.showToast({
              title: '请输入有效的数量',
              icon: 'none'
            });
            return;
          }
          
          if (quantity > maxPeople) {
            wx.showToast({
              title: `购买数量不能超过活动限制的${maxPeople}人`,
              icon: 'none'
            });
            return;
          }
          
          // 计算总价格
          const totalPrice = ticketPrice * quantity;
          
          // 跳转到支付页面
          wx.navigateTo({
            url: `/pages/pay/pay?totalPrice=${totalPrice}`
          });
        }
      }
    });
  },

  previewImage: function() {
    const image = this.data.activity.image;
    if (image) {
      wx.previewImage({
        current: image,
        urls: [image]
      });
    }
  }
});
