const api = require('../../utils/api'); 

Page({ 
  data: { 
    cartList: [], 
    totalPrice: '0.00', 
    selectedCount: 0, 
    showModal: false,
    showCartDetail: false,
    selectAll: false,
    addressList: [],
    selectedAddress: null,
    // 按类型分组
    regions: {
      food: { name: '点餐', selected: false, items: [], totalPrice: '0.00' },
      goods: { name: '商品', selected: false, items: [], totalPrice: '0.00' }
    },
    // 到店吃选项
    isTakeAway: false
  }, 

  onLoad() { 
    this.restoreCart(); 
    this.getUserAddressList();
  }, 

  onShow() { 
    this.restoreCart(); 
    this.getUserAddressList();
  },

  getUserAddressList() {
    api.api.user.getAddressList()
      .then((addresses) => {
        console.log('用户地址列表:', addresses);
        if (addresses && addresses.length > 0) {
          // 查找默认地址或选中第一个地址
          const defaultAddress = addresses.find(item => item.isDefault) || addresses[0];
          this.setData({
            addressList: addresses,
            selectedAddress: defaultAddress.id
          });
        } else {
          this.setData({
            addressList: [],
            selectedAddress: null
          });
        }
      })
      .catch((err) => {
        console.error('获取地址列表失败:', err);
        this.setData({
          addressList: [],
          selectedAddress: null
        });
      });
  }, 

  restoreCart() { 
    const cartList = (wx.getStorageSync('cartList') || []).map(item => ({ 
      ...item, 
      checked: !!item.checked, 
      count: Number(item.count || 0),
      // 添加默认值
      type: item.type || 'goods' // food: 点餐, goods: 商品
    })); 

    this.setData({ cartList }); 
    this.groupItemsByRegion();
    this.calcTotal(); 
  },

  // 按类型分组商品
  groupItemsByRegion() {
    const cartList = this.data.cartList;
    const regions = {
      food: { name: '点餐', selected: false, items: [], totalPrice: '0.00' },
      goods: { name: '商品', selected: false, items: [], totalPrice: '0.00' }
    };

    // 按照商品类型分组
    cartList.forEach(item => {
      if (item.type === 'food') {
        regions.food.items.push(item);
      } else {
        regions.goods.items.push(item);
      }
    });

    // 检查每个区域是否有选中的商品，并计算每个区域的总价格
    Object.keys(regions).forEach(key => {
      const region = regions[key];
      region.selected = region.items.some(item => item.checked);
      // 计算区域总价格
      const totalPrice = region.items
        .filter(item => item.checked)
        .reduce((total, item) => total + (item.price * item.count), 0);
      region.totalPrice = totalPrice.toFixed(2);
    });

    this.setData({ regions });
  },



  syncCart(cartList) { 
    const normalizedCartList = cartList 
      .filter(item => Number(item.count || 0) > 0) 
      .map(item => ({ 
        id: String(item.id), 
        name: item.name || '', 
        price: Number(item.price || 0), 
        image: item.image || '', 
        count: Number(item.count || 0), 
        checked: !!item.checked,
        type: item.type || 'goods' 
      })); 

    this.setData({ cartList: normalizedCartList }); 
    this.groupItemsByRegion();
    this.calcTotal(); 
    wx.setStorageSync('cartList', normalizedCartList); 

    api.request({ 
      url: '/api/cart/items', 
      method: 'POST', 
      data: { 
        cartList: normalizedCartList 
      } 
    }) 
      .then((data) => { 
        const nextCartList = (data.cartList || []).map(item => ({ 
          ...item, 
          checked: !!item.checked, 
          count: Number(item.count || 0),
          type: item.type || 'goods' 
        })); 

        this.setData({ cartList: nextCartList }); 
        this.groupItemsByRegion();
        this.calcTotal(); 
        wx.setStorageSync('cartList', nextCartList); 
      }) 
      .catch((err) => { 
        console.error('sync cart failed:', err); 
        wx.showToast({ 
          title: '购物车同步失败', 
          icon: 'none' 
        }); 
      }); 
  }, 

  handleMinus(e) { 
    const id = String(e.currentTarget.dataset.id); 
    const cartList = this.data.cartList.map(item => ({ ...item })); 
    const itemIndex = cartList.findIndex(item => String(item.id) === id); 

    if (itemIndex === -1) { 
      return; 
    } 

    if (cartList[itemIndex].count <= 1) { 
      cartList.splice(itemIndex, 1); 
    } else { 
      cartList[itemIndex].count -= 1; 
    } 

    this.syncCart(cartList); 
  }, 

  handlePlus(e) {
    const id = String(e.currentTarget.dataset.id);
    const cartList = this.data.cartList.map(item => ({ ...item }));
    const itemIndex = cartList.findIndex(item => String(item.id) === id);

    if (itemIndex === -1) {
      return;
    }

    // 确保商品数量不超过库存上限
    const stock = cartList[itemIndex].stock || 10;
    if (cartList[itemIndex].count < stock) {
      cartList[itemIndex].count += 1;
      this.syncCart(cartList);
    } else {
      wx.showToast({ 
        title: '已达到库存上限', 
        icon: 'none' 
      });
    }
  }, 

  toggleSelect(e) { 
    const id = String(e.currentTarget.dataset.id); 
    const cartList = this.data.cartList.map(item => ({ ...item })); 
    const itemIndex = cartList.findIndex(item => String(item.id) === id); 

    if (itemIndex === -1) { 
      return; 
    } 

    cartList[itemIndex].checked = !cartList[itemIndex].checked; 
    this.syncCart(cartList); 
  }, 

  calcTotal() { 
    let totalPrice = 0; 
    let selectedCount = 0; 

    this.data.cartList.forEach((item) => { 
      if (item.checked) { 
        totalPrice += Number(item.price || 0) * Number(item.count || 0); 
        selectedCount += Number(item.count || 0); 
      } 
    }); 

    const selectAll = this.data.cartList.length > 0 && this.data.cartList.every(item => item.checked); 

    this.setData({ 
      totalPrice: totalPrice.toFixed(2), 
      selectedCount,
      selectAll
    }); 

 
  }, 

  handleSettle() { 
    if (this.data.selectedCount === 0) { 
      wx.showToast({ 
        title: '请先选择商品', 
        icon: 'none' 
      }); 
      return; 
    } 

    // 先关闭购物车详情弹窗
    if (this.data.showCartDetail) {
      this.setData({ showCartDetail: false });
    }

    // 然后显示结算弹窗
    this.setData({ showModal: true }); 
  }, 

  handleConfirmPurchase() { 
    // 检查是否选择了地址（仅外卖需要）
    if (!this.data.isTakeAway && this.data.regions.food.selected && !this.data.selectedAddress) {
      wx.showToast({
        title: '请选择收货地址',
        icon: 'none'
      });
      return;
    }

    // 检查是否有选中的商品
    if (this.data.selectedCount === 0) {
      wx.showToast({
        title: '请选择商品',
        icon: 'none'
      });
      return;
    }

    // 检查选中的区域
    const hasFood = this.data.regions.food.selected;
    const hasGoods = this.data.regions.goods.selected;

    this.setData({ showModal: false }, () => { 
      // 如果只选中了点餐区域
      if (hasFood && !hasGoods) {
        if (this.data.isTakeAway) {
          // 到店吃订单
          wx.navigateTo({ 
            url: '/subpkg/buy/buy?orderId=1' 
          });
        } else {
          // 外卖订单
          wx.navigateTo({ 
            url: '/subpkg/order-foods/order-foods' 
          });
        }
      }
      // 如果只选中了商品区域
      else if (!hasFood && hasGoods) {
        wx.navigateTo({ 
          url: '/subpkg/buy/buy?orderId=1' 
        });
      }
      // 如果同时选中了点餐和商品区域
      else if (hasFood && hasGoods) {
        // 这里可以添加分别结算的逻辑
        // 先结算点餐
        if (this.data.isTakeAway) {
          // 到店吃订单
          wx.navigateTo({ 
            url: '/subpkg/buy/buy?orderId=1' 
          });
        } else {
          // 外卖订单
          wx.navigateTo({ 
            url: '/subpkg/order-foods/order-foods' 
          });
        }
      }
    }); 
  }, 

  handleCancelModal() { 
    this.setData({ showModal: false }); 
  },

 

  navTo(e) { 
    const pageMap = { 
      home: '/pages/index/index', 
      activity: '/pages/activity/activity', 
      cart: '/pages/cart/cart', 
      mine: '/pages/profile/profile' 
    }; 

    wx.switchTab({ 
      url: pageMap[e.currentTarget.dataset.page] 
    }); 
  },

  handleSelectAll() {
    const selectAll = !this.data.selectAll;
    const cartList = this.data.cartList.map(item => ({
      ...item,
      checked: selectAll
    }));
    this.syncCart(cartList);
  },

  handleClearCart() {
    wx.showModal({
      title: '确认清空',
      content: '确定要清空购物车吗？',
      success: (res) => {
        if (res.confirm) {
          this.setData({ 
            cartList: [],
            selectAll: false
          });
          this.calcTotal();
          wx.removeStorageSync('cartList');
          wx.showToast({ title: '购物车已清空', icon: 'success' });
        }
      }
    });
  },

  handleCartIconClick() {
    this.setData({ showCartDetail: true });
  },

  handleCloseCartDetail() {
    this.setData({ showCartDetail: false });
  },

  selectAddress(e) {
    const addressId = e.currentTarget.dataset.id;
    this.setData({
      selectedAddress: addressId
    });
  },

  addAddress() {
    wx.navigateTo({
      url: '/subpkg/address/address'
    });
  },

  // 切换区域选中状态
  toggleRegionSelect(e) {
    const region = e.currentTarget.dataset.region;
    const cartList = this.data.cartList.map(item => ({
      ...item,
      // 如果商品属于当前区域，切换其选中状态
      checked: item.type === region ? !this.data.regions[region].selected : item.checked
    }));
    
    this.syncCart(cartList);
  },

  // 切换到店吃选项
  toggleTakeAway(e) {
    this.setData({
      isTakeAway: e.detail.value
    });
  } 
});