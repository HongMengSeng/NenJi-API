const api = require('../../utils/api');

Page({
  data: {
    addressList: []
  },

  onLoad: function () {
    this.getAddressList();
  },
  
  onShow: function () {
    // 页面显示时自动刷新地址列表
    this.getAddressList();
  },

  // ─────────────────────────────────────────
  // 地址管理逻辑
  // ─────────────────────────────────────────
  getAddressList: function () {
    wx.showLoading({ title: '加载中...' });
    api.request({
      url: '/api/user/address',
      method: 'GET'
    })
    .then(data => {
      this.setData({ addressList: data || [] });
    })
    .catch(err => {
      console.error('获取地址列表失败:', err);
      wx.showToast({ title: '加载失败', icon: 'none' });
    })
    .finally(() => {
      wx.hideLoading();
    });
  },

  selectAddress: function (e) {
    console.log('点击地址项，跳转到编辑页面:', e.currentTarget.dataset.id);
    const addressId = e.currentTarget.dataset.id;
    wx.navigateTo({ url: `/subpkg/address-edit/address-edit?id=${addressId}` });
  },

  addAddress: function () {
    wx.navigateTo({ url: '/subpkg/address-edit/address-edit' });
  },

  editAddress: function (e) {
    const addressId = e.currentTarget.dataset.id;
    wx.navigateTo({ url: `/subpkg/address-edit/address-edit?id=${addressId}` });
  },

  deleteAddress: function (e) {
    const addressId = e.currentTarget.dataset.id;
    wx.showModal({
      title: '确认删除',
      content: '确定要删除这个收货地址吗？',
      success: (res) => {
        if (res.confirm) {
          wx.showLoading({ title: '删除中...' });
          api.request({
            url: '/api/user/address',
            method: 'DELETE',
            data: { id: addressId }
          })
          .then(() => {
            wx.showToast({ title: '删除成功', icon: 'success' });
            this.getAddressList();
          })
          .catch(err => {
            console.error('删除地址失败:', err);
            wx.showToast({ title: '删除失败', icon: 'none' });
          })
          .finally(() => {
            wx.hideLoading();
          });
        }
      }
    });
  },

  goBack: function () {
    wx.navigateBack();
  }
});
