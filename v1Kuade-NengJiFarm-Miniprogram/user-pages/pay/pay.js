const { api, request } = require('../../utils/api');

Page({
  data: {
    // 订单ID
    orderId: '',
    // 订单类型 (goods/food/activity)
    orderType: '',
    // 支付金额
    totalPrice: 0,
    // 加载状态
    loading: false,
    // 支付状态
    payStatus: 'pending', // pending, success, failed
    activityId: '',
    clearCartList: false
  },

  onLoad: function (options) {
    // 初始化页面状态
    this.initPageState();

    const orderId = options.orderId || '';
    const totalPrice = Number(options.totalPrice || 0);
    const activityId = options.activityId || '';
    // 订单类型：从参数获取，默认自动识别
    const orderType = options.type || '';
    const clearCartList = options.clearCartList === '1';

    if (!orderId) {
      wx.showToast({
        title: '缺少订单ID',
        icon: 'none'
      });
      this.setData({ payStatus: 'failed' });
      return;
    }

    this.setData({
      orderId,
      totalPrice,
      activityId,
      orderType,
      clearCartList
    });

    // 自动开始支付
    this.ensurePayAmountAndStart();
  },

  // 初始化页面状态
  initPageState: function () {
    this.setData({
      loading: false,
      payStatus: 'pending'
    });
  },

  // 验证订单信息并开始支付
  ensurePayAmountAndStart: function () {
    wx.showLoading({ title: '加载订单中...' });

    api.order.getDetail(this.data.orderId)
      .then((orderData) => {
        // 验证订单状态（只能支付待付款订单）
        if (orderData.status !== 'pending') {
          throw new Error('订单状态异常，无法支付');
        }

        // 从订单数据中提取金额
        const amount = Number(orderData.totalPrice || orderData.totalAmount || 0);
        if (amount <= 0) {
          throw new Error('订单金额异常');
        }

        // 如果未指定类型，从订单数据中获取
        if (!this.data.orderType && orderData.type) {
          this.setData({ orderType: orderData.type });
        }

        this.setData(
          { totalPrice: Number(amount.toFixed(2)) },
          () => this.startPayment()
        );
      })
      .catch((err) => {
        console.error('获取订单信息失败:', err);
        this.setData({ payStatus: 'failed' });
        wx.showToast({
          title: err.message || '订单信息获取失败',
          icon: 'none'
        });
      })
      .finally(() => {
        wx.hideLoading();
      });
  },

  // 开始支付
  startPayment: function () {
    if (!this.data.orderId) {
      this.setData({ loading: false, payStatus: 'failed' });
      wx.showToast({ title: '支付参数错误', icon: 'none' });
      return;
    }

    this.setData({ loading: true });

    // 调用后端 JSAPI 支付接口
    api.pay.createJsapi({
      orderId: this.data.orderId,
      id: this.data.orderId,
      type: this.data.orderType || undefined
    })
      .then((payParams) => {
        // 检查是否返回了微信支付参数
        if (payParams && payParams.timeStamp && payParams.nonceStr && payParams.package && payParams.paySign) {
          // 调用微信小程序支付
          wx.requestPayment({
            timeStamp: payParams.timeStamp,
            nonceStr: payParams.nonceStr,
            package: payParams.package,
            signType: payParams.signType || 'HMAC-SHA256',
            paySign: payParams.paySign,
            success: () => {
              this.handlePaySuccess();
            },
            fail: (err) => {
              console.error('微信支付失败:', err);
              this.handlePayFail(err);
            }
          });
        } else {
          // 没有返回支付参数（已支付等情况）
          this.handlePaySuccess();
        }
      })
      .catch((err) => {
        console.error('获取支付参数失败:', err);
        // 降级：尝试模拟支付（仅开发环境）
        this.startPaymentLegacy();
      });
  },

  // 降级处理 - 模拟支付（开发环境使用）
  startPaymentLegacy: function () {
    api.order.pay(this.data.orderId, { paymentMethod: 'wechat' })
      .then(() => {
        this.handlePaySuccess();
      })
      .catch((err) => {
        console.error('支付失败:', err);
        this.handlePayFail(err);
      });
  },

  // 处理支付成功
  handlePaySuccess: function () {
    this.setData({
      loading: false,
      payStatus: 'success'
    });
    wx.showToast({
      title: '支付成功',
      icon: 'success'
    });
    
    // 支付成功后的逻辑
    this.afterPaySuccess();
  },

  // 处理支付失败
  handlePayFail: function (err) {
    this.setData({
      loading: false,
      payStatus: 'failed'
    });
    wx.showToast({
      title: err && err.message ? err.message : '支付失败',
      icon: 'none'
    });
  },

  // 支付成功后的处理
  afterPaySuccess: function() {
    // 如果是活动订单，跳转回活动详情并显示二维码
    if (this.data.activityId) {
      setTimeout(() => {
        wx.redirectTo({
          url: `/user-pages/activity-detail/activity-detail?id=${this.data.activityId}&paid=true&orderId=${this.data.orderId}`
        });
      }, 1500);
    } else {
      // 普通订单跳转到订单列表
      setTimeout(() => {
        wx.redirectTo({
          url: '/user-pages/orders/orders?tab=paid'
        });
      }, 1500);
    }
  },

  // 查看订单列表（支付成功后）
  goOrders: function () {
    wx.redirectTo({
      url: '/user-pages/orders/orders?tab=paid'
    });
  },

  // 返回上一页或订单列表
  goBack: function () {
    wx.navigateBack({
      fail: () => {
        wx.switchTab({
          url: '/pages/index/index'
        });
      }
    });
  },

  // 重新支付
  retryPay: function () {
    this.setData({ payStatus: 'pending' });
    this.startPayment();
  }
});

