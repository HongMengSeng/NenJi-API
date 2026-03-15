// 修改备注：统一封装后端请求，约定后端成功格式为 { code: 0, data: ... }
const BASE_URL = 'http://localhost:5162';

function request({ url, method = 'GET', data = {} }) {
  return new Promise((resolve, reject) => {
    const token = wx.getStorageSync('token');
    const header = {
      'Content-Type': 'application/json'
    };

    if (token) {
      header.Authorization = 'Bearer ' + token;
    }

    wx.request({
      url: BASE_URL + url,
      method,
      data,
      header,
      success(res) {
        if (res.data && res.data.code === 0) {
          resolve(res.data.data);
          return;
        }

        const msg = res.data && res.data.message ? res.data.message : '请求出错';
        wx.showToast({
          title: msg,
          icon: 'none'
        });
        reject(res.data);
      },
      fail(err) {
        wx.showToast({
          title: '网络错误',
          icon: 'none'
        });
        reject(err);
      }
    });
  });
}

module.exports = {
  request
};
