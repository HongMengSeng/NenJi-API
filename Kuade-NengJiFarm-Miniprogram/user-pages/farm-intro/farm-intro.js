Page({
  data: {
    farmInfo: null,
    loading: true,
    // 默认农场主图
    defaultMainImage: 'http://192.168.203.56/api/file/image/farm_0000000000007.jpg'
  },

  onLoad: function (options) {
    console.log('农场介绍页面加载');
    // 直接使用默认农场信息
    this.setData({
      farmInfo: {
        mainImage: this.data.defaultMainImage,
        introduction: '能记家庭农场致力于提供绿色、健康、有机的农产品，采用传统种植方式，不使用化学农药和化肥，确保产品的品质和安全。',
        philosophy: '我们坚持"自然、健康、可持续"的发展理念，致力于为消费者提供最优质的农产品，同时保护生态环境，实现农业的可持续发展。',
        contact: {
          address: '广东省广州市从化区',
          phone: '15876534944',
          email: 'njjtnc@example.com'
        }
      },
      loading: false
    });
  },

  // 处理图片路径，确保使用正确的基础 URL
  processImageUrl: function (imageUrl) {
    if (!imageUrl) return '';
    
    // 去除反引号和空格
    imageUrl = imageUrl.replace(/[`\s]/g, '');
    
    // 如果是完整的 URL，替换基础 URL
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      // 替换开发环境地址为生产环境地址
      imageUrl = imageUrl.replace('http://127.0.0.1:5000', 'http://192.168.203.56');
      // 如果URL包含 /images/farm/，转换为 /api/file/image/
      if (imageUrl.includes('/images/farm/')) {
        const fileName = imageUrl.split('/images/farm/')[1];
        return 'http://192.168.203.56/api/file/image/' + fileName;
      }
      return imageUrl;
    }
    
    // 如果是相对路径，使用 /api/file/image/ API
    let fileName = imageUrl;
    if (fileName.startsWith('/')) {
      fileName = fileName.substring(1);
    }
    // 如果路径包含 /images/farm/，只提取文件名
    if (fileName.includes('/images/farm/')) {
      fileName = fileName.split('/images/farm/')[1];
    } else if (fileName.includes('images/farm/')) {
      fileName = fileName.split('images/farm/')[1];
    }
    return 'http://192.168.203.56/api/file/image/' + fileName;
  }
});