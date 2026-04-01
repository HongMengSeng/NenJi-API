const api = require('../../utils/api');
const QQMapWX = require('../../utils/qqmap-wx-jssdk.min.js');
const qqmapsdk = new QQMapWX({
  key: '6R6BZ-7SHJW-WDPKT-LTC4N-6UXD6-48RVW'
});

Page({
  data: {
    addressId: null,
    formData: {
      name: '',
      phone: '',
      province: '',
      city: '',
      district: '',
      address: '',
      detail: '',
      isDefault: false
    }
  },

  onLoad: function (options) {
    console.log('编辑地址页面加载', options);
    if (options.id) {
      this.setData({ addressId: options.id });
      this.loadAddressDetail(options.id);
    }
  },

  // 加载地址详情
  loadAddressDetail: function (id) {
    wx.showLoading({ title: '加载中...' });
    
    api.request({
      url: `/api/address/${id}`,
      method: 'GET'
    })
    .then(data => {
      this.setData({
        formData: {
          name: data.name,
          phone: data.phone,
          province: data.province,
          city: data.city,
          district: data.district,
          address: data.address || (data.province + data.city + data.district),
          detail: data.detail,
          isDefault: data.isDefault
        }
      });
    })
    .catch(err => {
      console.error('获取地址详情失败:', err);
      wx.showToast({ title: '加载失败', icon: 'none' });
    })
    .finally(() => {
      wx.hideLoading();
    });
  },

  // 输入框输入事件
  onInputChange: function (e) {
    const { field } = e.currentTarget.dataset;
    const { value } = e.detail;
    
    this.setData({
      [`formData.${field}`]: value
    });
  },

  // 切换默认地址
  toggleDefault: function () {
    this.setData({
      'formData.isDefault': !this.data.formData.isDefault
    });
  },

  // 选择地址
  chooseRegion: function () {
    let that = this;
    // 1. 先判断用户是否授权位置权限
    wx.getSetting({
      success(res) {
        // 未授权 -> 引导授权
        if (!res.authSetting['scope.userLocation']) {
          wx.authorize({
            scope: 'scope.userLocation',
            success() {
              // 授权成功 -> 选择位置
              that.chooseLocation();
            },
            fail() {
              // 授权失败 -> 提示去设置页开启
              wx.showModal({
                title: '提示',
                content: '请开启位置权限',
                confirmText: '去设置',
                success(res) {
                  if (res.confirm) {
                    wx.openSetting();
                  }
                }
              })
            }
          })
        } else {
          // 已授权 -> 直接选择位置
          that.chooseLocation();
        }
      }
    })
  },

  // 使用微信 chooseLocation 获取地址
  chooseLocation: function () {
    let that = this;
    wx.showLoading({ title: '获取位置中...' });

    wx.chooseLocation({
      success: function(res) {
        wx.hideLoading();
        console.log('选择位置结果：', res);
        
        // 解析地址信息
        const address = res.address;
        const name = res.name;
        
        // 提取省市区信息和详细地址
        let province = '';
        let city = '';
        let district = '';
        let detail = '';
        
        // 详细解析地址
        console.log('原始地址:', address);
        console.log('地点名称:', name);
        
        // 处理直辖市和省份
        console.log('开始解析地址:', address);
        
        // 尝试多种地址解析方式
        // 方式1：使用正则表达式
        let cityMatch = address.match(/^(北京市|上海市|天津市|重庆市|\w+省)/);
        
        if (cityMatch) {
          if (cityMatch[1].includes('省')) {
            // 省份
            province = cityMatch[1];
            const remainingAfterProvince = address.substring(province.length);
            console.log('省份:', province, '剩余:', remainingAfterProvince);
            
            // 提取城市
            let cityMatch2 = remainingAfterProvince.match(/^(\w+市|\w+州|\w+盟)/);
            if (cityMatch2) {
              city = cityMatch2[1];
              const remainingAfterCity = remainingAfterProvince.substring(city.length);
              console.log('城市:', city, '剩余:', remainingAfterCity);
              
              // 提取区县
              let districtMatch = remainingAfterCity.match(/^(\w+区|\w+县|\w+旗|\w+市)/);
              if (districtMatch) {
                district = districtMatch[1];
                console.log('区县:', district);
              }
            }
          } else {
            // 直辖市
            province = cityMatch[1];
            city = cityMatch[1];
            console.log('直辖市:', province);
            
            // 提取区县
            const remainingAfterCity = address.substring(city.length);
            let districtMatch = remainingAfterCity.match(/^(\w+区|\w+县|\w+旗|\w+市)/);
            if (districtMatch) {
              district = districtMatch[1];
              console.log('区县:', district);
            }
          }
        } else {
          // 方式2：尝试从地址中提取省市区
          console.log('正则匹配失败，尝试提取省市区');
          
          // 提取省份
          const provinceMatch = address.match(/(\w+省)/);
          if (provinceMatch) {
            province = provinceMatch[1];
          }
          
          // 提取城市
          const cityMatch2 = address.match(/(\w+市|\w+州|\w+盟)/);
          if (cityMatch2) {
            city = cityMatch2[1];
          }
          
          // 提取区县
          const districtMatch = address.match(/(\w+区|\w+县|\w+旗|\w+市)/);
          if (districtMatch) {
            district = districtMatch[1];
          }
          
          console.log('提取结果:', { province, city, district });
          
          // 如果还是失败，使用完整地址
          if (!province) {
            console.log('提取失败，使用完整地址');
            province = address;
            city = '';
            district = '';
          }
        }
        
        // 提取详细地址
        let detailedAddress = address;
        if (province) {
          detailedAddress = detailedAddress.replace(province, '');
        }
        if (city && city !== province) {
          detailedAddress = detailedAddress.replace(city, '');
        }
        if (district) {
          detailedAddress = detailedAddress.replace(district, '');
        }
        
        // 详细地址 = 地点名称 + 详细地址
        detail = name + (detailedAddress ? ' ' + detailedAddress.trim() : '');
        
        // 确保省市区字段只包含省、市、区信息
        // 从完整地址中提取省市区
        let provinceOnly = province;
        let cityOnly = city;
        let districtOnly = district;
        
        // 如果省市区字段包含详细地址信息，重新提取
        if (province && (province.includes('路') || province.includes('街') || province.includes('道') || province.includes('村'))) {
          // 尝试从地址中提取省市区
          const provinceMatch = address.match(/(\w+省)/);
          if (provinceMatch) {
            provinceOnly = provinceMatch[1];
          }
          
          const cityMatch = address.match(/(\w+市|\w+州|\w+盟)/);
          if (cityMatch) {
            cityOnly = cityMatch[1];
          }
          
          const districtMatch = address.match(/(\w+区|\w+县|\w+旗|\w+市)/);
          if (districtMatch) {
            districtOnly = districtMatch[1];
          }
          
          console.log('重新提取省市区:', { provinceOnly, cityOnly, districtOnly });
        }
        
        // 更新省市区变量
        province = provinceOnly;
        city = cityOnly;
        district = districtOnly;
        
        console.log('解析结果:', { province, city, district, detail });
        
        console.log('准备设置的值:', {
          province: province,
          city: city,
          district: district,
          detail: detail
        });
        
        // 确保省市区值不为空
        console.log('设置前的formData:', that.data.formData);
        console.log('准备设置的省市区:', { province, city, district });
        
        // 绑定完整的地址到address字段
        // 确保不重复显示省市区信息
        const fullAddress = [province, city, district].filter(Boolean).join('');
        console.log('生成完整地址:', fullAddress);
        
        that.setData({
          'formData.province': province,
          'formData.city': city,
          'formData.district': district,
          'formData.address': fullAddress,
          'formData.detail': detail
        });
        
        console.log('设置完整地址:', fullAddress);
        
        // 立即检查设置后的值
        console.log('设置后的值(立即):', that.data.formData);
        
        // 检查设置后的值
        setTimeout(() => {
          console.log('设置后的值:', that.data.formData);
        }, 100);
      },
      fail: function(err) {
        wx.hideLoading();
        console.error('选择位置失败:', err);
        wx.showToast({ title: '获取位置失败', icon: 'none' });
      }
    });
    
  },

  // 保存地址
  saveAddress: function () {
    const { formData } = this.data;
    
    // 表单验证
    if (!formData.name) {
      wx.showToast({ title: '请输入收件人姓名', icon: 'none' });
      return;
    }
    
    if (!formData.phone) {
      wx.showToast({ title: '请输入手机号', icon: 'none' });
      return;
    }
    
    // 手机号验证
    const phoneReg = /^1[3-9]\d{9}$/;
    if (!phoneReg.test(formData.phone)) {
      wx.showToast({ title: '请输入正确的手机号', icon: 'none' });
      return;
    }
    
    if (!formData.address) {
      wx.showToast({ title: '请选择所在地区', icon: 'none' });
      return;
    }
    
    if (!formData.detail) {
      wx.showToast({ title: '请输入详细地址', icon: 'none' });
      return;
    }
    
    wx.showLoading({ title: '保存中...' });
    
    // 准备请求数据
    const requestData = {
      name: formData.name,
      phone: formData.phone,
      province: formData.province || formData.address,
      city: formData.city || formData.address,
      district: formData.district || formData.address,
      address: formData.address || formData.detail,
      isDefault: formData.isDefault
    };
    
    // 如果是修改地址，添加id字段
    if (this.data.addressId) {
      requestData.id = this.data.addressId;
    }
    
    const requestConfig = {
      method: this.data.addressId ? 'PUT' : 'POST',
      url: '/api/user/address',
      data: requestData
    };
    
    api.request(requestConfig)
    .then(() => {
      wx.showToast({ title: '保存成功', icon: 'success' });
      // 返回上一页
      wx.navigateBack();
    })
    .catch(err => {
      console.error('保存地址失败:', err);
      wx.showToast({ title: '保存失败', icon: 'none' });
    })
    .finally(() => {
      wx.hideLoading();
    });
  },

  // 返回上一页
  goBack: function () {
    wx.navigateBack();
  }
});