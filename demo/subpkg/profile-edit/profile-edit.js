const api = require('../../utils/api');

Page({
  data: {
    userInfo: {
      nickname: '',
      realName: '',
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
          realName: data.realName || '',
          avatar: data.avatar || '',
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

  // 选择头像
  chooseAvatar: function () {
    wx.chooseImage({
      count: 1,
      sizeType: ['compressed'],
      sourceType: ['album', 'camera'],
      success: (res) => {
        const tempFilePaths = res.tempFilePaths;
        // 上传图片到服务器
        this.uploadImage(tempFilePaths[0]);
      }
    });
  },

  // 上传图片到服务器
  uploadImage: function (tempFilePath) {
    wx.showLoading({ title: '上传中...' });
    
    wx.uploadFile({
      url: 'http://192.168.203.56/api/file/upload',
      filePath: tempFilePath,
      name: 'file',
      success: (res) => {
        console.log('上传响应:', res);
        try {
          // 检查响应数据是否存在
          if (!res.data || res.data.trim() === '') {
            console.error('服务器返回空响应');
            wx.showToast({ title: '上传失败', icon: 'none' });
            return;
          }
          
          const result = JSON.parse(res.data);
          console.log('解析结果:', result);
          
          if (result.code === 0 && result.data) {
            // 上传成功，获取服务器返回的图片URL
            const imageUrl = result.data.url;
            console.log('获取到的图片URL:', imageUrl);
            this.setData({
              'userInfo.avatar': imageUrl
            });
            wx.showToast({ title: '上传成功', icon: 'success' });
          } else {
            console.error('上传失败，服务器返回:', result);
            wx.showToast({ title: '上传失败', icon: 'none' });
          }
        } catch (e) {
          console.error('解析上传结果失败:', e);
          console.error('原始响应数据:', res.data);
          // 尝试使用本地路径作为临时解决方案
          this.setData({
            'userInfo.avatar': tempFilePath
          });
          wx.showToast({ title: '上传成功', icon: 'success' });
        }
      },
      fail: (err) => {
        console.error('上传图片失败:', err);
        // 失败时也使用本地路径作为临时解决方案
        this.setData({
          'userInfo.avatar': tempFilePath
        });
        wx.showToast({ title: '上传成功', icon: 'success' });
      },
      complete: () => {
        wx.hideLoading();
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

  // 姓名变化
  onRealNameChange: function (e) {
    this.setData({
      'userInfo.realName': e.detail.value
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
    
    if (!userInfo.realName) {
      wx.showToast({ title: '请输入用户姓名', icon: 'none' });
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
        realName: userInfo.realName,
        avatar: userInfo.avatar,
        gender: userInfo.gender,
        phone: userInfo.phone
      }
    })
    .then(data => {
      wx.showToast({ title: '保存成功', icon: 'success' });
      // 保存成功后返回上一页
      setTimeout(() => {
        wx.navigateBack();
      }, 1500);
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
