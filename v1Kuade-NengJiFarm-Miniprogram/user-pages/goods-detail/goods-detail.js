const { api, request } = require('../../utils/api');
const utils = require('../../utils/utils'); // 统一引入工具函数

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
      storage: '',
      videoUrl: '',
      stock: 0
    },
    cartQuantity: 0,
    swiperList: [],
    hasVideo: false,
    loading: true,
    cartCount: 0,
    showBuyModal: false,
    addressList: [],
    selectedAddress: null,
    defaultAddress: null,
    showAllAddresses: false,
    quantity: 1,
    totalPrice: '0'
  },

  // 处理图片/视频链接
  processImageUrl(url) {
    return utils.media.processUrl(url);
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
    this.getAddressList(); // 从地址编辑页返回时更新地址列表
  },

  // 获取商品详情
  getGoodsDetail(goodsId) {
    wx.showLoading({ title: '加载中...' });

    request({
      url: '/api/goods/detail',
      method: 'GET',
      showLoading: false,
      data: { goodsId }
    })
      .then(data => {
        // 视频地址处理
        const rawVideo = data.videoUrl || data.video || data.video_url || '';
        const videoUrl = rawVideo
          ? (String(rawVideo).startsWith('http') ? rawVideo : this.processImageUrl(String(rawVideo)))
          : '';
        const hasVideo = !!videoUrl;

        console.log('商品详情原始数据:', JSON.stringify(data));

        // 图片地址处理
        const goodsImage = this.processImageUrl(data.image) || '';
        const detailImage = this.processImageUrl(data.detailImage) || goodsImage;

        // 轮播图列表，至少保证展示详情图和商品图
        let swiperList = (data.swiperList || []).map(item => ({
          ...item,
          image: this.processImageUrl(item.image)
        }));
        if (swiperList.length === 0 && detailImage) {
          swiperList = [
            { id: 1, image: detailImage },
            { id: 2, image: goodsImage }
          ];
        }

        // 价格清洗（去除非数字和小数点）
        const rawPrice = data.price || 0;
        const price = Number(String(rawPrice).replace(/[^\d.]/g, '')) || 0;

        this.setData({
          goods: {
            id: data.id || goodsId,
            name: data.name || '',
            price,
            image: goodsImage,
            detailImage,
            description: data.description || '',
            weight: data.weight || '',
            storage: data.storage || '',
            videoUrl,
            stock: Number(data.stock || 0)
          },
          swiperList,
          loading: false,
          hasVideo
        });
      })
      .catch(err => {
        console.error('获取商品详情失败:', err);
        this.setData({ loading: false });
      })
      .finally(() => {
        wx.hideLoading();
      });
  },

  // 更新购物车数量
  updateCartCount() {
    const cart = wx.getStorageSync('cartList') || {};
    let count = 0;
    Object.keys(cart).forEach(key => {
      const item = cart[key];
      count += item.count || item.quantity || 0;
    });

    const goodsId = String(this.data.goods.id);
    const goodsInCart = cart[goodsId];
    const cartQuantity = goodsInCart ? (goodsInCart.count || goodsInCart.quantity || 0) : 0;

    this.setData({ cartCount: count, cartQuantity });
  },

  // 加入购物车
  addToCart() {
    const goods = this.data.goods;
    const cart = wx.getStorageSync('cartList') || {};
    const id = String(goods.id);
    const currentQty = cart[id] ? (cart[id].count || cart[id].quantity || 0) : 0;

    // 库存不足时禁止添加（无论库存是否为0）
    if (currentQty >= goods.stock) {
      wx.showToast({ title: '库存不足', icon: 'none' });
      return;
    }

    const nextCart = { ...cart };
    if (nextCart[id]) {
      nextCart[id].count = currentQty + 1;
      nextCart[id].quantity = nextCart[id].count;
    } else {
      nextCart[id] = {
        id,
        name: goods.name,
        price: goods.price,
        image: goods.image,
        count: 1,
        quantity: 1,
        checked: true,
        stock: goods.stock
      };
    }

    wx.setStorageSync('cartList', nextCart);
    this.updateCartCount();
    wx.showToast({ title: '已加入购物车', icon: 'success' });
  },

  // 立即购买
  buyNow() {
    this.setData({ showBuyModal: true, quantity: 1 });
    this.calculateTotalPrice();
  },

  hideBuyModal() {
    this.setData({ showBuyModal: false });
  },

  // 增加购买数量（校验库存）
  increaseQuantity() {
    if (this.data.quantity >= this.data.goods.stock) {
      wx.showToast({ title: '库存不足', icon: 'none' });
      return;
    }
    this.setData({ quantity: this.data.quantity + 1 }, () => {
      this.calculateTotalPrice();
    });
  },

  // 减少购买数量
  decreaseQuantity() {
    if (this.data.quantity > 1) {
      this.setData({ quantity: this.data.quantity - 1 }, () => {
        this.calculateTotalPrice();
      });
    }
  },

  // 计算总价
  calculateTotalPrice() {
    const total = (this.data.goods.price * this.data.quantity).toFixed(2);
    this.setData({ totalPrice: total });
  },

  // 数量输入实时处理
  onQuantityInput(e) {
    let val = e.detail.value;
    if (val === '') return;
    let num = parseInt(val, 10);
    if (isNaN(num)) num = 1;
    if (num > this.data.goods.stock) {
      num = this.data.goods.stock;
      wx.showToast({ title: `最多购买${this.data.goods.stock}件`, icon: 'none' });
    }
    this.setData({ quantity: num }, () => {
      this.calculateTotalPrice();
    });
  },

  // 数量输入失焦确认
  onQuantityBlur(e) {
    let val = e.detail.value;
    let num = parseInt(val, 10);
    if (isNaN(num) || num < 1) num = 1;
    if (num > this.data.goods.stock) {
      num = this.data.goods.stock;
      wx.showToast({ title: `最多购买${this.data.goods.stock}件`, icon: 'none' });
    }
    this.setData({ quantity: num }, () => {
      this.calculateTotalPrice();
    });
  },

  // 获取地址列表
  getAddressList() {
    request({
      url: '/api/user/address',
      method: 'GET',
      showLoading: false
    })
      .then(data => {
        const addressList = data || [];
        const defaultAddress = addressList.find(addr => addr.isDefault) || addressList[0] || null;
        this.setData({
          addressList,
          selectedAddress: defaultAddress ? defaultAddress.id : null,
          defaultAddress
        });
      })
      .catch(err => {
        console.error('获取地址列表失败:', err);
        // 用户未登录或接口异常时静默处理，不影响商品展示
      });
  },

  // 选择地址
  selectAddress(e) {
    const id = e.currentTarget.dataset.id;
    const address = this.data.addressList.find(addr => addr.id === id);
    if (address) {
      this.setData({
        selectedAddress: id,
        defaultAddress: address,
        showAllAddresses: false
      });
    }
  },

  // 切换地址列表显示
  toggleAddressList() {
    this.setData({ showAllAddresses: !this.data.showAllAddresses });
  },

  // 跳转新增地址
  addAddress() {
    wx.navigateTo({
      url: '/user-pages/address-edit/address-edit'
    });
  },

  // 编辑地址
  editAddress(e) {
    const id = e.currentTarget.dataset.id;
    wx.navigateTo({
      url: `/user-pages/address-edit/address-edit?id=${id}`
    });
  },

  // 确认下单
  confirmBuy() {
    if (!this.data.selectedAddress) {
      wx.showToast({ title: '请选择地址', icon: 'none' });
      return;
    }

    const payload = {
      addressId: this.data.selectedAddress,
      items: [{
        id: parseInt(this.data.goods.id, 10),
        price: this.data.goods.price,
        quantity: this.data.quantity
      }]
    };

    wx.showLoading({ title: '创建订单中...' });
    request({
      url: '/api/commodity-order/create',
      method: 'POST',
      showLoading: false,
      data: payload
    })
      .then(data => {
        const orderId = data.orderId || data.id;
        wx.navigateTo({
          url: `/user-pages/orders-detail/orders-detail?id=${encodeURIComponent(orderId)}`
        });
      })
      .catch(err => {
        // request 已全局处理错误提示，此处可做额外处理
        console.error('创建订单失败:', err);
      })
      .finally(() => {
        wx.hideLoading();
      });
  },

  // 预览轮播图大图
  previewImage(e) {
    const current = e.currentTarget.dataset.url;
    const urls = this.data.swiperList.map(item => item.image);
    wx.previewImage({ current, urls });
  },

  // 预览详情图
  previewDetailImages(e) {
    const current = e.currentTarget.dataset.url;
    const urls = [this.data.goods.detailImage, this.data.goods.image].filter(Boolean);
    wx.previewImage({ current, urls });
  },

  // 前往购物车
  goToCart() {
    wx.switchTab({ url: '/pages/cart/cart' });
  }
});
