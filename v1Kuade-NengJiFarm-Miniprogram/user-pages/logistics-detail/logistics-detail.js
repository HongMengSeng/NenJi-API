// 引入物流查询插件
const plugin = requirePlugin("logisticsPlugin");
const { api } = require('../../utils/api');

Page({
  data: {
    orderId: '',
    orderNo: '',
    waybillId: '',
    deliveryId: '',
    deliveryName: '',
    openId: '',
    transId: '',
    loading: false,
    loadingText: '加载中...',
    errorMsg: '',
    orderItems: [],
    goodsList: []
  },

  onLoad(options) {
    const orderId = options.orderId || '';
    if (!orderId) {
      this.setData({ errorMsg: '缺少订单信息' });
      return;
    }

    this.setData({
      orderId: orderId,
      orderNo: options.orderNo || orderId,
      waybillId: options.waybillId || '',
      deliveryId: options.deliveryId || '',
      deliveryName: options.deliveryName || '',
      transId: options.transId || '',
      openId: wx.getStorageSync('openid') || wx.getStorageSync('openId') || ''
    });

    // 如果已经有运单号和快递公司编码，直接准备查看物流
    if (this.data.waybillId && this.data.deliveryId) {
      // 尝试加载商品信息（从订单详情获取）
      this.loadGoodsInfo();
    } else {
      // 缺少物流信息，先尝试从接口获取
      this.fetchLogisticsInfo();
    }
  },

  // 从订单/物流接口获取运单信息
  fetchLogisticsInfo() {
    this.setData({ loading: true, loadingText: '获取物流信息...', errorMsg: '' });

    api.logistics.getDetail(this.data.orderId)
      .then(data => {
        if (data) {
          const waybillId = data.waybillNo || data.waybillId || '';
          const deliveryId = data.companyCode || data.deliveryId || data.expressCode || '';
          const deliveryName = data.companyName || data.expressCompany || data.deliveryName || '';
          const items = data.items || [];

          this.setData({
            waybillId,
            deliveryId,
            deliveryName,
            orderItems: items,
            goodsList: (items || []).map(item => {
              const entry = { goodsName: item.name || '能记农场商品' };
              if (item.image && !item.image.includes('192.168.')) {
                entry.goodsImgUrl = item.image;
              }
              return entry;
            }),
            loading: false
          });

          // 自动调用物流插件
          if (waybillId && deliveryId) {
            this.viewLogistics();
          } else {
            this.setData({ errorMsg: '暂无物流信息' });
          }
        } else {
          this.setData({ loading: false, errorMsg: '暂无物流信息' });
        }
      })
      .catch(err => {
        console.error('获取物流信息失败:', err);
        this.setData({ loading: false, errorMsg: '获取物流信息失败，请稍后重试' });
      });
  },

  // 加载商品信息（作为查看物流时的商品列表展示）
  loadGoodsInfo() {
    api.order.getDetail(this.data.orderId)
      .then(orderData => {
        if (orderData && orderData.items) {
          const items = orderData.items;
          this.setData({
            orderItems: items,
            goodsList: items.map(item => ({
              goodsName: item.name || '能记农场商品',
              goodsImgUrl: item.image || ''
            }))
          });
        }
      })
      .catch(() => {});
  },

  // 查看物流详情
  viewLogistics() {
    const { waybillId, deliveryId, openId, transId, goodsList, orderId } = this.data;

    if (!waybillId || !deliveryId) {
      // 回到登录页自动获取
      this.fetchLogisticsInfo();
      return;
    }

    this.setData({ loading: true, loadingText: '加载物流中...' });

    // 调用后端接口获取 waybill_token
    api.logistics.getWaybillToken({
      openId: openId,
      waybillId: waybillId,
      deliveryId: deliveryId,
      receiverPhone: '',  // 后端根据订单获取
      transId: transId,
      goodsList: goodsList.length > 0 ? goodsList : [{ goodsName: '能记农场商品' }]
    })
      .then(data => {
        const waybillToken = data.waybillToken || data;
        if (waybillToken) {
          // 调用微信官方插件打开物流详情页
          plugin.openWaybillTracking({
            waybillToken: waybillToken,
            success: () => {
              console.log('打开物流详情成功');
              this.setData({ loading: false });
            },
            fail: (err) => {
              console.error('打开物流详情失败：', err);
              this.setData({ loading: false, errorMsg: '打开物流失败，请重试' });
            }
          });
        } else {
          this.setData({ loading: false, errorMsg: '获取物流信息失败' });
        }
      })
      .catch(err => {
        console.error('请求异常：', err);
        const msg = err.message || '';
        if (msg.includes('access_token') || msg.includes('credential')) {
          this.setData({ loading: false, errorMsg: '物流服务配置异常，请联系管理员' });
        } else {
          this.setData({ loading: false, errorMsg: msg || '网络异常，请稍后重试' });
        }
      });
  },

  // 重试
  retry() {
    this.setData({ errorMsg: '', loading: false });
    this.fetchLogisticsInfo();
  },

  goBack() {
    wx.navigateBack();
  }
});
