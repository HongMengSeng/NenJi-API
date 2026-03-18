// simple wrapper for HTTP requests to the backend
const BASE_URL = 'http://192.168.203.56';

function request({ url, method = 'GET', data = {} }) {
  return new Promise((resolve, reject) => {
    const token = wx.getStorageSync('token');
    const header = {
      'Content-Type': 'application/json'
    };
    if (token) {
      header.Authorization = 'Bearer ' + token;
    }

    const requestUrl = /^https?:\/\//i.test(url) ? url : BASE_URL + url;

    wx.request({
      url: requestUrl,
      method,
      data,
      header,
      success(res) {
        if (res.data && res.data.code === 0) {
          resolve(res.data.data);
        } else {
          const msg = res.data && res.data.message ? res.data.message : '请求出错';
          wx.showToast({ title: msg, icon: 'none' });
          reject(res.data);
        }
      },
      fail(err) {
        wx.showToast({ title: '网络错误', icon: 'none' });
        reject(err);
      }
    });
  });
}

module.exports = { request };