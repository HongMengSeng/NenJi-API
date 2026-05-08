const { api } = require('../../utils/api');
// 引入物流查询插件
const plugin = requirePlugin("logisticsPlugin");

Page({
  data: {
    orderId: '',
    loading: true,
    logisticsInfo: {},
    logisticsTrace: [],
    shippingAddress: null,
    orderItems: [],
    statusIcon: '🚚',
    statusHint: '您的包裹正在运输中',
    shipTime: '',
    estimatedArrival: '',
    // 物流插件所需数据
    waybillId: '',
    deliveryId: '',
    deliveryName: '',
    transId: '',
    pluginLoading: false
  },

  onLoad(options) {
    const orderId = options.orderId;
    if (!orderId) {
      wx.showToast({
        title: '缺少订单ID',
        icon: 'none'
      });
      setTimeout(() => {
        wx.navigateBack();
      }, 1500);
      return;
    }

    this.setData({ orderId });
    this.getLogisticsDetail(orderId);
  },

  // 获取物流详情
  getLogisticsDetail(orderId) {
    wx.showLoading({ title: '加载中...' });

    // 物流API已包含 shippingAddress / items / shipTime / estimatedArrival，优先使用
    api.logistics.getDetail(orderId)
      .then((data) => {
        console.log('获取物流详情成功:', data);
        this.setLogisticsData(data);
        return this.getLogisticsTrace(orderId);
      })
      .catch((err) => {
        console.warn('获取物流详情失败，尝试订单API:', err);
        // 物流API失败，回退到订单详情
        return api.order.getDetail(orderId)
          .then((orderData) => {
            this.setMockLogisticsData(orderId, orderData);
            return this.getLogisticsTrace(orderId);
          })
          .catch(() => {
            this.setMockLogisticsData(orderId, null);
            return this.getLogisticsTrace(orderId);
          });
      })
      .finally(() => {
        wx.hideLoading();
      });
  },

  // 获取物流轨迹
  getLogisticsTrace(orderId) {
    return api.logistics.getTrace(orderId)
      .then((traceData) => {
        console.log('获取物流轨迹成功:', traceData);
        this.setLogisticsTrace(traceData);
      })
      .catch((err) => {
        console.warn('获取物流轨迹失败:', err);
        this.setData({ logisticsTrace: [] });
      });
  },

  // 根据状态获取图标和提示语
  _getStatusMeta(status) {
    const map = {
      'pending':      { icon: '📦', hint: '商家正在准备发货' },
      'paid':         { icon: '📦', hint: '商家正在准备发货' },
      'picked':       { icon: '📦', hint: '快递员已揽收' },
      'shipping':     { icon: '🚚', hint: '您的包裹正在运输中' },
      'transporting': { icon: '🚚', hint: '您的包裹正在运输中' },
      'delivering':   { icon: '🚚', hint: '快递员正在派送中' },
      'delivered':    { icon: '✅', hint: '包裹已签收' },
      'completed':    { icon: '✅', hint: '包裹已签收' },
      'signed':       { icon: '✅', hint: '包裹已签收' }
    };
    return map[status] || { icon: '🚚', hint: '物流信息更新中' };
  },

  // 设置物流数据
  setLogisticsData(data) {
    const meta = this._getStatusMeta(data.status);
    const statusIcon = meta.icon;
    // 优先使用API返回的 statusText，否则用本地映射的 hint
    const statusHint = data.statusText || meta.hint;

    // 处理商品图片
    let processedItems = [];
    if (data.items) {
      processedItems = data.items.map(item => ({
        ...item,
        image: this.processImageUrl(item.image)
      }));
    }

    this.setData({
      logisticsInfo: data,
      shippingAddress: data.shippingAddress || null,
      orderItems: processedItems,
      statusIcon: statusIcon,
      statusHint: statusHint,
      shipTime: data.shipTime || '',
      estimatedArrival: data.estimatedArrival || '',
      loading: false
    });
  },

  // 设置物流轨迹
  setLogisticsTrace(traceData) {
    let trace = [];
    if (Array.isArray(traceData)) {
      trace = traceData;
    } else if (traceData && Array.isArray(traceData.trace)) {
      trace = traceData.trace;
    }

    this.setData({
      logisticsTrace: trace
    });
  },

  // 处理图片URL
  processImageUrl: function(imageUrl) {
    const utils = require('../../utils/utils');
    return utils.media.processUrl(imageUrl);
  },

  // 设置模拟物流数据
  setMockLogisticsData(orderId, orderData) {
    const now = new Date();
    
    // 处理商品图片
    let processedItems = [];
    if (orderData && orderData.items) {
      processedItems = orderData.items.map(item => ({
        ...item,
        image: this.processImageUrl(item.image)
      }));
    }
    
    // 判断订单状态
    const orderStatus = orderData ? orderData.status : '';
    const isPaidOnly = orderStatus === 'paid';
    
    // 确定状态
    let status = 'shipping';
    let statusText = '运输中';
    if (isPaidOnly) {
      status = 'pending';
      statusText = '待发货';
    } else if (orderData && (orderData.status === 'completed' || orderData.status === 'delivered')) {
      status = 'delivered';
      statusText = '已签收';
    }

    const meta = this._getStatusMeta(status);

    const logisticsData = {
      companyName: isPaidOnly ? '' : '顺丰速运',
      companyPhone: isPaidOnly ? '' : '95338',
      waybillNo: isPaidOnly ? '' : orderId,
      status: status,
      statusText: statusText,
      shippingAddress: orderData ? orderData.shippingAddress : {
        name: '收货人',
        phone: '13800138000',
        address: '广东省深圳市南山区'
      },
      items: processedItems
    };

    this.setData({
      logisticsInfo: logisticsData,
      shippingAddress: logisticsData.shippingAddress,
      orderItems: logisticsData.items,
      statusIcon: meta.icon,
      statusHint: meta.hint,
      loading: false
    });
  },

  copyWaybillNo() {
    wx.setClipboardData({
      data: this.data.logisticsInfo.waybillNo || '',
      success: () => {
        wx.showToast({ title: '单号已复制', icon: 'success' });
      }
    });
  },

  callCompany() {
    const phone = this.data.logisticsInfo.companyPhone || '95338';
    wx.makePhoneCall({
      phoneNumber: phone
    });
  },

  goBack() {
    wx.navigateBack();
  },

  // 查看物流详情（调用微信官方物流插件）
  viewLogisticsPlugin() {
    const { logisticsInfo, orderId } = this.data;

    // 检查是否有物流单号
    if (!logisticsInfo.waybillNo) {
      wx.showToast({ title: '暂无物流信息', icon: 'none' });
      return;
    }

    this.setData({ pluginLoading: true });

    // 获取openId
    const openId = wx.getStorageSync('openid') || wx.getStorageSync('openId') || '';

    // 调用后端接口获取waybill_token（使用新的API接口）
    api.logistics.getWaybillToken({
      openId: openId,
      waybillId: logisticsInfo.waybillNo,
      deliveryId: logisticsInfo.companyCode || 'SF', // 快递公司代码，默认顺丰
      receiverPhone: logisticsInfo.shippingAddress?.phone || '13800138000',
      transId: this.data.transId || '',
      goodsList: logisticsInfo.items?.map(item => ({
        goodsName: item.name || '能记农场商品',
        goodsImgUrl: item.image || ''
      })) || [{ goodsName: '能记农场商品', goodsImgUrl: '' }]
    })
      .then((data) => {
        const waybillToken = data.waybillToken;
        if (waybillToken) {
          // 调用微信官方插件打开物流详情页
          plugin.openWaybillTracking({
            waybillToken: waybillToken,
            success: () => {
              console.log('打开物流详情成功');
            },
            fail: (err) => {
              console.error('打开物流详情失败：', err);
              wx.showToast({ title: '打开失败', icon: 'none' });
            }
          });
        } else {
          wx.showToast({ title: '获取物流信息失败', icon: 'none' });
        }
      })
      .catch((err) => {
        console.error('获取物流token失败:', err);
        wx.showToast({ title: err.message || '获取物流信息失败', icon: 'none' });
      })
      .finally(() => {
        this.setData({ pluginLoading: false });
      });
  }
});
