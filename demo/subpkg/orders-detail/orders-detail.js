const { api } = require('../../utils/api');

Page({
  data: {
    order: {
      id: '',
      status: '',
      statusText: '',
      createTime: '',
      paymentTime: null,
      shippingTime: null,
      completeTime: null,
      totalPrice: 0,
      shippingAddress: {
        name: '',
        phone: '',
        address: ''
      },
      items: [],
      paymentMethod: null,
      transactionId: null
    },
    loading: true
  },

  onLoad(options) {
    const orderId = options.id;

    if (!orderId) {
      this.setData({ loading: false });
      wx.showToast({
        title: '缺少订单ID',
        icon: 'none'
      });
      return;
    }

    this.getOrderDetail(orderId);
  },

  getOrderDetail(orderId) {
    wx.showLoading({ title: '加载中...' });

    api.order.getDetail(orderId)
      .then((data) => {
        // 处理订单数据，添加活动订单标识和二维码
        const orderData = data || {
          id: orderId,
          status: '',
          statusText: '',
          createTime: '',
          paymentTime: null,
          shippingTime: null,
          completeTime: null,
          totalPrice: 0,
          shippingAddress: {
            name: '',
            phone: '',
            address: ''
          },
          items: [],
          paymentMethod: null,
          transactionId: null
        };
        
        // 检查是否为活动订单（这里假设活动商品名称包含'活动'或'亲子'等关键词）
        const isActivityOrder = orderData.items.some(item => 
          item.name.includes('活动') || item.name.includes('亲子')
        );
        
        // 检查是否为认购一亩订单（这里假设认购一亩商品名称包含'认购'或'一亩'等关键词）
        const isAcreOrder = orderData.items.some(item => 
          item.name.includes('认购') || item.name.includes('一亩')
        );
        
        // 为活动订单添加二维码（实际项目中应该从服务器获取）
        if (isActivityOrder) {
          orderData.isActivityOrder = true;
          // 使用临时二维码图片，实际项目中应该从服务器获取
          orderData.qrcode = 'http://192.168.203.56/api/file/image/farm_0000000000007.jpg';
        } else {
          orderData.isActivityOrder = false;
        }
        
        // 为认购一亩订单添加标识
        orderData.isAcreOrder = isAcreOrder;
        
        this.setData({
          order: orderData,
          loading: false
        });
      })
      .catch((err) => {
        console.error('获取订单详情失败:', err);
        this.setData({ loading: false });
        wx.showToast({
          title: '获取订单详情失败',
          icon: 'none'
        });
      })
      .finally(() => {
        wx.hideLoading();
      });
  },

  payOrder() {
    const orderId = this.data.order.id;
    wx.navigateTo({
      url: `/subpkg/pay/pay?orderId=${orderId}`
    });
  },

  goToOrders() {
    wx.navigateTo({
      url: '/subpkg/orders/orders'
    });
  },

  goBack() {
    wx.navigateBack();
  },

  // 取消订单
  cancelOrder() {
    wx.showModal({
      title: '确认取消',
      content: '确定要取消这个订单吗？',
      success: (res) => {
        if (res.confirm) {
          wx.showLoading({ title: '取消中...' });
          api.order.updateStatus(this.data.order.id, 'cancelled')
          .then(() => {
            wx.hideLoading();
            wx.showToast({ title: '订单已取消', icon: 'success' });
            this.getOrderDetail(this.data.order.id);
          })
          .catch((err) => {
            console.error('取消订单失败:', err);
            wx.hideLoading();
            wx.showToast({ title: '取消订单失败', icon: 'none' });
          });
        }
      }
    });
  },

  // 确认收货
  confirmReceipt() {
    wx.showModal({
      title: '确认收货',
      content: '确定已收到商品吗？',
      success: (res) => {
        if (res.confirm) {
          wx.showLoading({ title: '确认中...' });
          api.order.updateStatus(this.data.order.id, 'completed')
          .then(() => {
            wx.hideLoading();
            wx.showToast({ title: '收货成功', icon: 'success' });
            this.getOrderDetail(this.data.order.id);
          })
          .catch((err) => {
            console.error('确认收货失败:', err);
            wx.hideLoading();
            wx.showToast({ title: '确认收货失败', icon: 'none' });
          });
        }
      }
    });
  },

  // 模拟发货（联调用）
  markShipping() {
    wx.showModal({
      title: '确认发货',
      content: '确认将订单更新为待收货吗？',
      success: (res) => {
        if (!res.confirm) {
          return;
        }

        wx.showLoading({ title: '处理中...' });
        api.order.updateStatus(this.data.order.id, 'shipping')
          .then(() => {
            wx.showToast({ title: '已更新为待收货', icon: 'success' });
            this.getOrderDetail(this.data.order.id);
          })
          .catch((err) => {
            console.error('更新发货状态失败:', err);
            wx.showToast({ title: '更新失败', icon: 'none' });
          })
          .finally(() => {
            wx.hideLoading();
          });
      }
    });
  },

  // 申请退款
  applyRefund() {
    wx.showModal({
      title: '请联系能记家庭农场客服进行退款',
      content: '手机号：15876534944\n     微信号：njjtnc15876534944',
      showCancel: false
    });
  }
});
