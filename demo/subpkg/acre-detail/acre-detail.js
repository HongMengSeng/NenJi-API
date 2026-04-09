const api = require('../../utils/api');

Page({
  data: {
    acreDetail: {},
    swiperList: []
  },
  onLoad: function(options) {
    const id = options.id;
    this.loadAcreDetail(id);
  },
  
  loadAcreDetail: function(id) {
    wx.showLoading({
      title: '加载中...',
    });
    
    // 获取认购详情
    api.api.acre.getDetail(id)
    .then(acreData => {
      wx.hideLoading();
      
      // 清理图片路径中的反引号和空格
      const cleanData = {
        ...acreData.acreDetail,
        image: acreData.acreDetail.image ? acreData.acreDetail.image.replace(/[`\s]/g, '') : '',
        longExampleImage: acreData.acreDetail.longExampleImage ? acreData.acreDetail.longExampleImage.replace(/[`\s]/g, '') : '',
        swiperList: (acreData.acreDetail.swiperList || []).map(item => ({
          ...item,
          image: item.image ? item.image.replace(/[`\s]/g, '') : ''
        })),
        longExampleImages: (acreData.acreDetail.longExampleImages || []).map(image => image.replace(/[`\s]/g, '')),
        longExampleImageList: (acreData.acreDetail.longExampleImageList || []).map(image => image.replace(/[`\s]/g, '')),
        bottomImages: (acreData.acreDetail.bottomImages || []).map(image => image.replace(/[`\s]/g, ''))
      };
      
      // 模仿首页获取视频的方法
      const videoUrl = 'http://192.168.203.56/api/file/video/farm_intro.mp4';
      
      // 确保数据结构完整
      const acreDetail = {
        ...cleanData,
        videoUrl: videoUrl, // 视频URL
        remainingAcres: cleanData.remainingAcres , // 剩余亩数
        soldAcres: cleanData.soldAcres , // 已售亩数
        longExampleImage: cleanData.longExampleImage, // 农场示例图片
        bottomImages: cleanData.bottomImages, // 底部图片列表
        longExampleImages: cleanData.longExampleImages, // 长图示例
        longExampleImageList: cleanData.longExampleImageList // 长图列表
      };
      
      this.setData({
        acreDetail: acreDetail,
        swiperList: cleanData.swiperList // 轮播图数据
      });
    })
    .catch(err => {
      wx.hideLoading();
      wx.showToast({
        title: '加载失败，请重试',
        icon: 'none'
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
    const price = this.data.acreDetail.price || '¥0';
    
    // 检查是否已卖完
    if (remainingAcres <= 0) {
      wx.showToast({
        title: '已卖完',
        icon: 'none'
      });
      return;
    }
    
    wx.showModal({
      title: `当前剩余 ${remainingAcres} 亩，${price}`,
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
            const price = parseFloat(String(this.data.acreDetail.price || 0).replace(/[^0-9.]/g, '')) || 0;
            const totalPrice = price * acres;

            wx.showLoading({ title: '下单中...' });
            api.api.order.create({
              sourceType: 'acre',
              sourceName: this.data.acreDetail.title || '认购',
              quantity: acres,
              totalPrice: Number(totalPrice.toFixed(2)),
              items: [
                {
                  id: String(this.data.acreDetail.id || ''),
                  name: this.data.acreDetail.title || '认购',
                  price: Number((price || 0).toFixed(2)),
                  quantity: acres,
                  image: this.data.acreDetail.image || ''
                }
              ]
            })
              .then((orderData) => {
                const orderId = orderData.orderId || orderData.id;
                if (!orderId) {
                  wx.showToast({
                    title: '创建订单失败',
                    icon: 'none'
                  });
                  return;
                }

                wx.navigateTo({
                  url: `/subpkg/pay/pay?orderId=${orderId}&totalPrice=${totalPrice.toFixed(2)}`
                });
              })
              .catch((err) => {
                console.error('创建认购订单失败:', err);
                wx.showToast({
                  title: '创建订单失败',
                  icon: 'none'
                });
              })
              .finally(() => {
                wx.hideLoading();
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
