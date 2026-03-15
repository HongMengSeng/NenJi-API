// 修改备注：本文件已调整为对接后端真实微信登录接口 /api/Home/wxlogin
// 修改备注：登录成功后会同时缓存 token、userGuid、openid、userInfo，便于后续页面复用

const api = require('../../utils/api');

function saveLoginResult(result) {
  if (!result || !result.token) {
    return false;
  }

  const userGuid =
    result.userGuid ||
    (result.userInfo && result.userInfo.userNo) ||
    (result.user && result.user.userNo) ||
    '';

  wx.setStorageSync('token', result.token);
  wx.setStorageSync('hasLogin', true);
  wx.setStorageSync('userGuid', userGuid);
  wx.setStorageSync('user_guid', userGuid);
  wx.setStorageSync('openid', result.openid || '');
  wx.setStorageSync('userInfo', result.userInfo || result.user || {});
  return true;
}

function clearLoginState() {
  wx.removeStorageSync('token');
  wx.removeStorageSync('hasLogin');
  wx.removeStorageSync('userGuid');
  wx.removeStorageSync('user_guid');
  wx.removeStorageSync('openid');
  wx.removeStorageSync('userInfo');
}

Page({
  data: {
    isLogging: false
  },

  onLoad() {
    // 修改备注：进入页面后优先调用后端校验接口，而不是只看本地 token
    this.checkLoginStatus();
  },

  checkLoginStatus() {
    const token = wx.getStorageSync('token');
    if (!token) {
      clearLoginState();
      return;
    }

    api.request({
      url: '/api/Auth/check',
      method: 'GET'
    })
      .then(() => {
        wx.switchTab({
          url: '/pages/index/index'
        });
      })
      .catch(() => {
        clearLoginState();
      });
  },

  oneClickLogin() {
    // 修改备注：页面按钮仍保留原方法名，但内部已切换为真实微信登录流程
    this.wechatLogin();
  },

  wechatLogin() {
    if (this.data.isLogging) return;
    this.setData({ isLogging: true });

    wx.showLoading({
      title: '登录中...',
      mask: true
    });

    wx.login({
      success: (res) => {
        if (!res.code) {
          wx.hideLoading();
          this.setData({ isLogging: false });
          wx.showToast({
            title: '获取code失败',
            icon: 'none'
          });
          return;
        }

        api.request({
          // 修改备注：这里改成当前后端可用的真实微信登录接口
          url: '/api/Auth/wxlogin',
          method: 'POST',
          data: {
            code: res.code,
            encryptedData: '',
            iv: '',
            nickname: '',
            avatar: ''
          }
        })
          .then((data) => {
            const saved = saveLoginResult(data);
            if (!saved) {
              wx.showToast({
                title: '登录返回缺少token',
                icon: 'none'
              });
              return;
            }

            wx.showToast({
              title: '登录成功',
              icon: 'success'
            });

            setTimeout(() => {
              wx.switchTab({
                url: '/pages/index/index'
              });
            }, 800);
          })
          .catch((err) => {
            wx.showToast({
              title: (err && err.message) || '登录失败',
              icon: 'none'
            });
          })
          .finally(() => {
            wx.hideLoading();
            this.setData({ isLogging: false });
          });
      },
      fail: () => {
        wx.hideLoading();
        this.setData({ isLogging: false });
        wx.showToast({
          title: '微信登录失败',
          icon: 'none'
        });
      }
    });
  },

  // 修改备注：手机号登录示例暂不删除，保留给后续扩展使用
  getPhoneNumber(e) {
    if (e.detail.errMsg !== 'getPhoneNumber:ok') {
      wx.showToast({
        title: '您拒绝了授权',
        icon: 'none'
      });
      return;
    }

    const code = e.detail.code;
    wx.request({
      url: 'http://192.168.1.12:5141/api/GetPhone',
      method: 'POST',
      data: { code },
      success: (res) => {
        if (res.data.success) {
          wx.showToast({
            title: '获取成功',
            icon: 'success'
          });
          console.log('手机号：', res.data.purePhoneNumber);
        } else {
          wx.showToast({
            title: res.data.msg || '获取失败',
            icon: 'none'
          });
        }
      },
      fail: () => {
        wx.showToast({
          title: '网络错误',
          icon: 'none'
        });
      }
    });
  },

  viewAgreement() {
    wx.showModal({
      title: '用户协议',
      content: '欢迎使用农场小程序，请在登录前阅读并同意用户协议。',
      showCancel: false,
      confirmText: '我知道了'
    });
  },

  viewPrivacy() {
    wx.showModal({
      title: '隐私政策',
      content: '我们会按照隐私政策保护你的个人信息，请放心使用。',
      showCancel: false,
      confirmText: '我知道了'
    });
  }
});
