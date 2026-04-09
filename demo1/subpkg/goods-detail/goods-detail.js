const api = require('../../utils/api');

Page({
  data: {
    goods: {
      id: '',
      name: '',
      price: 0,
      image: '',
      detailImage: '',
      description: '',
      weight: '',
      storage: ''
    },
    loading: true,
    cartCount: 0,
    showBuyModal: false,
    addressList: [],
    selectedAddress: null
  },

  onLoad(options) {
    const goodsId = options.id;

    if (!goodsId) {
      this.setData({ loading: false });
      wx.showToast({
        title: '缺少商品ID',
        icon: 'none'
      });
      return;
    }

    this.getGoodsDetail(goodsId);
    this.updateCartCount();
    this.getAddressList();
  },

  onShow() {
    this.updateCartCount();
    // 重新获取地址列表，确保从地址编辑页面返回时能看到最新的地址
    this.getAddressList();
  },

  getGoodsDetail(goodsId) {
    wx.showLoading({ title: '加载中...' });

    api.api.goods.getDetail(goodsId)
      .then((data) => {
        this.setData({
          goods: {
            id: data.id || goodsId,
            name: data.name || '',
            price: Number(data.price || 0),
            image: data.image || '',
            detailImage: data.detailImage || data.image || '',
            description: data.description || '',
            weight: data.weight || '',
            storage: data.storage || ''
          },
          loading: false
        });
      })
      .catch((err) => {
        console.error('获取商品详情失败:', err);
        this.setData({ loading: false });
      })
      .finally(() => {
        wx.hideLoading();
      });
  },

  addToCart() {
    const goods = this.data.goods;
    const currentCart = wx.getStorageSync('cartList') || [];
    const nextCart = currentCart.map(item => ({ ...item }));
    const targetId = String(goods.id);
    const existingIndex = nextCart.findIndex(item => String(item.id) === targetId);

    if (existingIndex > -1) {
      nextCart[existingIndex].count += 1;
      nextCart[existingIndex].checked = true;
    } else {
      nextCart.push({
        id: targetId,
        name: goods.name,
        price: Number(goods.price || 0),
        image: goods.image,
        tag: goods.tag || '',
        count: 1,
        checked: true
      });
    }

    wx.showLoading({ title: '加入中...' });

    api.api.cart.add({
      goodsId: goods.id,
      count: 1
    })
      .then((data) => {
        // 即使API返回的数据结构不同，也要更新本地购物车
        wx.setStorageSync('cartList', nextCart);
        this.updateCartCount();
        wx.showToast({
          title: '已加入购物车',
          icon: 'success'
        });
      })
      .catch((err) => {
        console.error('加入购物车失败:', err);
      })
      .finally(() => {
        wx.hideLoading();
      });
  },

  buyNow() {
    this.setData({ showBuyModal: true });
  },

  hideBuyModal() {
    this.setData({ showBuyModal: false });
  },

  getAddressList() {
    api.api.user.getAddressList()
      .then((data) => {
        const addressList = Array.isArray(data) ? data : [];
        const processedAddressList = addressList.map((address, index) => ({
          id: address.id || String(index + 1),
          name: address.name || '',
          phone: address.phone || '',
          address: address.address || '',
          isDefault: address.isDefault || false
        }));
        
        const defaultAddress = processedAddressList.find(item => item.isDefault);
        const selectedAddressId = defaultAddress ? defaultAddress.id : (processedAddressList.length > 0 ? processedAddressList[0].id : null);
        
        this.setData({
          addressList: processedAddressList,
          selectedAddress: selectedAddressId
        });
      }).catch((err) => {
        console.error('获取地址列表失败:', err);
        this.setData({ addressList: [], selectedAddress: null });
      });
  },

  selectAddress(e) {
    const addressId = e.currentTarget.dataset.id;
    this.setData({ selectedAddress: addressId });
  },

  addAddress() {
    wx.navigateTo({
      url: '/subpkg/address-edit/address-edit'
    });
  },

  confirmBuy() {
    if (!this.data.selectedAddress) {
      wx.showToast({
        title: '请选择收货地址',
        icon: 'none'
      });
      return;
    }

    const selectedAddressInfo = this.data.addressList.find(
      item => item.id === this.data.selectedAddress
    );

    if (!selectedAddressInfo) {
      wx.showToast({
        title: '地址信息错误',
        icon: 'none'
      });
      return;
    }

    wx.showLoading({ title: '提交订单中...' });

    api.api.order.create({
      sourceType: 'goods',
      sourceName: this.data.goods.name,
      quantity: 1,
      totalPrice: Number(this.data.goods.price || 0),
      address: {
        addressId: selectedAddressInfo.id,
        name: selectedAddressInfo.name,
        phone: selectedAddressInfo.phone,
        address: selectedAddressInfo.address
      },
      items: [
        {
          id: String(this.data.goods.id || ''),
          name: this.data.goods.name || '',
          price: Number(this.data.goods.price || 0),
          quantity: 1,
          image: this.data.goods.image || ''
        }
      ]
    }).then((data) => {
      wx.hideLoading();
      this.setData({ showBuyModal: false });
      const orderId = data.orderId || data.id;
      if (!orderId) {
        wx.showToast({
          title: '创建订单失败',
          icon: 'none'
        });
        return;
      }
      wx.navigateTo({
        url: '/subpkg/orders/orders?tab=pending'
      });
    }).catch((err) => {
      wx.hideLoading();
      console.error('创建订单失败:', err);
      wx.showToast({
        title: '创建订单失败',
        icon: 'none'
      });
    });
  },

  updateCartCount() {
    const cartList = wx.getStorageSync('cartList') || [];
    let totalCount = 0;
    cartList.forEach(item => {
      totalCount += item.count || 0;
    });
    this.setData({ cartCount: totalCount });
  },

  goToCart() {
    wx.switchTab({
      url: '/pages/cart/cart'
    });
  }
});
