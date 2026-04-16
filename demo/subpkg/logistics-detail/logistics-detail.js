const { api } = require('../../utils/api');

Page({
  data: {
    orderId: '',
    loading: true,
    logisticsInfo: {},
    logisticsTrace: [],
    shippingAddress: null,
    orderItems: [],
    statusIcon: '🚚'
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

    // 先尝试获取物流详情
    api.logistics.getDetail(orderId)
      .then((data) => {
        console.log('获取物流详情成功:', data);
        this.setLogisticsData(data);
        return this.getLogisticsTrace(orderId);
      })
      .catch((err) => {
        console.warn('获取物流详情失败，使用模拟数据:', err);
        // 如果API失败，使用模拟数据
        this.setMockLogisticsData(orderId);
        return this.getLogisticsTrace(orderId);
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
        console.warn('获取物流轨迹失败，使用模拟数据:', err);
        // 如果API失败，使用模拟轨迹数据
        this.setMockLogisticsTrace();
      });
  },

  // 设置物流数据
  setLogisticsData(data) {
    let statusIcon = '🚚';
    if (data.status === 'delivered' || data.status === 'completed') {
      statusIcon = '✅';
    } else if (data.status === 'shipping' || data.status === 'transporting') {
      statusIcon = '🚚';
    } else if (data.status === 'picked') {
      statusIcon = '📦';
    }

    this.setData({
      logisticsInfo: data,
      shippingAddress: data.shippingAddress || null,
      orderItems: data.items || [],
      statusIcon: statusIcon,
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

  // 设置模拟物流数据（用于API未就绪时）
  setMockLogisticsData(orderId) {
    this.setData({
      logisticsInfo: {
        companyName: '顺丰快递',
        waybillNo: orderId,
        statusText: '运输中',
        status: 'shipping'
      },
      shippingAddress: {
        name: '张三',
        phone: '138****8000',
        address: '广东省广州市越秀区农科街道新河浦路123号'
      },
      orderItems: [
        {
          id: 1,
          name: '农家橘子',
          price: '9.9',
          quantity: 1,
          image: 'http://192.168.203.56/api/file/image/farm_000000000001.jpg'
        }
      ],
      statusIcon: '🚚',
      loading: false
    });
  },

  // 设置模拟物流轨迹（用于API未就绪时）
  setMockLogisticsTrace() {
    this.setData({
      logisticsTrace: [
        {
          time: '2026-04-16 15:30:00',
          desc: '快件已到达【广州转运中心】',
          location: '广州市'
        },
        {
          time: '2026-04-16 10:20:00',
          desc: '快件已从【深圳集散中心】发出',
          location: '深圳市'
        },
        {
          time: '2026-04-16 08:15:00',
          desc: '快件已到达【深圳集散中心】',
          location: '深圳市'
        },
        {
          time: '2026-04-15 18:30:00',
          desc: '快件已从【深圳营业部】发出',
          location: '深圳市'
        },
        {
          time: '2026-04-15 16:00:00',
          desc: '快递员已揽收',
          location: '深圳市'
        }
      ]
    });
  },

  // 复制运单号
  copyWaybillNo() {
    const waybillNo = this.data.logisticsInfo.waybillNo || this.data.orderId;
    if (!waybillNo) {
      wx.showToast({
        title: '暂无运单号',
        icon: 'none'
      });
      return;
    }

    wx.setClipboardData({
      data: waybillNo,
      success: () => {
        wx.showToast({
          title: '复制成功',
          icon: 'success'
        });
      },
      fail: () => {
        wx.showToast({
          title: '复制失败',
          icon: 'none'
        });
      }
    });
  },

  // 返回上一页
  goBack() {
    wx.navigateBack();
  }
});
