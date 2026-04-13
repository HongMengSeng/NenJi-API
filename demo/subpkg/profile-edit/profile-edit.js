const api = require('../../utils/api');

Page({
  data: {
    userInfo: {
      nickname: '',
      avatar: '',
      gender: '',
      phone: ''
    }
  },

  onLoad: function () {
    console.log('基本信息页面加载');
    this.getUserProfile();
  },

  // 获取用户信息
  getUserProfile: function () {
    wx.showLoading({ title: '加载中...' });
    
    api.request({
      url: '/api/user/profile',
      method: 'GET'
    })
    .then(data => {
      this.setData({
        userInfo: {
          nickname: data.nickname || '',
          avatar: this.processImageUrl(data.avatar || ''),
          gender: data.gender || '保密',
          phone: data.phone || ''
        }
      });
    })
    .catch(err => {
      console.error('获取用户信息失败:', err);
      wx.showToast({ title: '加载失败', icon: 'none' });
    })
    .finally(() => {
      wx.hideLoading();
    });
  },

  // 处理图片路径，确保使用正确的基础 URL
  processImageUrl: function (imageUrl) {
    if (!imageUrl) return '';
    
    // 去除反引号和空格
    imageUrl = imageUrl.replace(/[`\s]/g, '');
    
    // 如果是完整的 URL，替换基础 URL
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      // 替换 127.0.0.1:5000 为 192.168.203.56
      return imageUrl.replace('http://127.0.0.1:5000', 'http://192.168.203.56');
    }
    
    // 如果是相对路径，添加基础 URL
    return 'http://192.168.203.56' + imageUrl;
  },

  // 选择头像（使用微信官方组件）
  onChooseAvatar: function (e) {
    const avatarUrl = e.detail.avatarUrl;
    
    // 上传图片到服务器
    wx.uploadFile({
      url: 'http://192.168.203.56/api/upload',
      filePath: avatarUrl,
      name: 'file',
      success: (res) => {
        try {
          // 检查响应数据是否为空
          if (!res.data || res.data.trim() === '') {
            console.error('服务器返回空响应');
            // 直接使用临时路径作为 fallback，不显示错误提示
            this.setData({
              'userInfo.avatar': avatarUrl
            });
            return;
          }
          
          const data = JSON.parse(res.data);
          if (data.code === 0) {
            // 使用服务器返回的图片URL
            this.setData({
              'userInfo.avatar': this.processImageUrl(data.data.url)
            });
          } else {
            console.error('上传头像失败:', data.message);
            wx.showToast({ title: '上传头像失败', icon: 'none' });
            // 失败时使用临时路径作为 fallback
            this.setData({
              'userInfo.avatar': avatarUrl
            });
          }
        } catch (e) {
          console.error('解析上传结果失败:', e);
          wx.showToast({ title: '上传头像失败', icon: 'none' });
          // 失败时使用临时路径作为 fallback
          this.setData({
            'userInfo.avatar': avatarUrl
          });
        }
      },
      fail: (err) => {
        console.error('上传头像失败:', err);
        wx.showToast({ title: '上传头像失败', icon: 'none' });
        // 失败时使用临时路径作为 fallback
        this.setData({
          'userInfo.avatar': avatarUrl
        });
      }
    });
  },

  // 选择性别
  chooseGender: function () {
    wx.showActionSheet({
      itemList: ['男', '女', '保密'],
      success: (res) => {
        const genders = ['男', '女', '保密'];
        this.setData({
          'userInfo.gender': genders[res.tapIndex]
        });
      }
    });
  },

  // 昵称变化
  onNicknameChange: function (e) {
    this.setData({
      'userInfo.nickname': e.detail.value
    });
  },

  // 手机号变化
  onPhoneChange: function (e) {
    this.setData({
      'userInfo.phone': e.detail.value
    });
  },

  // 保存用户信息
  saveProfile: function () {
    const userInfo = this.data.userInfo;
    
    // 表单验证
    if (!userInfo.nickname) {
      wx.showToast({ title: '请输入用户昵称', icon: 'none' });
      return;
    }
    
    if (!userInfo.phone) {
      wx.showToast({ title: '请输入手机号码', icon: 'none' });
      return;
    }
    
    // 手机号格式验证
    const phoneRegex = /^1[3-9]\d{9}$/;
    if (!phoneRegex.test(userInfo.phone)) {
      wx.showToast({ title: '请输入正确的手机号码', icon: 'none' });
      return;
    }
    
    wx.showLoading({ title: '保存中...' });
    
    api.request({
      url: '/api/user/profile',
      method: 'PUT',
      data: {
        nickname: userInfo.nickname,
        avatar: userInfo.avatar,
        gender: userInfo.gender,
        phone: userInfo.phone
      }
    })
    .then(() => {
      const profileCache = {
        nickname: userInfo.nickname || '',
        avatar: userInfo.avatar || '',
        email: '',
        balance: 0,
        reward: 0
      };
      wx.setStorageSync('user_profile_cache', profileCache);

      const pages = getCurrentPages();
      const prevPage = pages.length > 1 ? pages[pages.length - 2] : null;
      if (prevPage && prevPage.route === 'pages/profile/profile') {
        prevPage.setData({
          userInfo: {
            nickname: profileCache.nickname,
            avatar: profileCache.avatar,
            email: prevPage.data.userInfo.email || '',
            balance: prevPage.data.userInfo.balance || 0,
            reward: prevPage.data.userInfo.reward || 0
          }
        });
      }

      wx.showToast({ title: '保存成功', icon: 'success' });
      // 保存成功后返回上一页
      setTimeout(() => {
        wx.navigateBack();
      }, 500);
    })
    .catch(err => {
      console.error('保存用户信息失败:', err);
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
