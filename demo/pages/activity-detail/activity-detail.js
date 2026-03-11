Page({
  data: {
    // 活动详情
    activity: {},
    // 加载状态
    loading: true
  },

  onLoad: function (options) {
    const activityId = options.id;
    if (activityId) {
      this.getActivityDetail(activityId);
    }
  },

  // 获取活动详情
  getActivityDetail: function(activityId) {
    wx.showLoading({ title: '加载中...' });
    
    // 模拟API调用
    setTimeout(() => {
      // 根据活动id返回不同的活动详情
      let activityData;
      
      switch(activityId) {
        case '1':
          activityData = {
            id: activityId,
            title: '农家研学活动报名中',
            price: '门票: 10-20 ¥',
            date: '2025.2.25-2025.3.6',
            image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=children%20playing%20football%20on%20farm&image_size=landscape_16_9',
            description: '农家研学活动是一项针对青少年的教育活动，通过参与农场劳动、了解农业知识，培养孩子们的动手能力和对自然的热爱。活动包括采摘体验、动物喂养、农产品制作等多个环节，让孩子们在快乐中学习。',
            location: '农场优选生态农场',
            people: '限50人',
            content: '1. 农场参观：了解农场的基本情况和种植养殖过程\n2. 采摘体验：亲手采摘新鲜的蔬菜和水果\n3. 动物喂养：与农场动物亲密接触\n4. 农产品制作：学习制作简单的农产品\n5. 农耕体验：体验传统农耕方式',
            images: [
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=children%20playing%20football%20on%20farm&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=children%20feeding%20farm%20animals&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=children%20picking%20vegetables&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=children%20learning%20farming&image_size=square'
            ]
          };
          break;
        case '2':
          activityData = {
            id: activityId,
            title: '采摘活动报名中',
            price: '门票: 10-50 ¥',
            date: '2025.2.25-2025.3.6',
            image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20lettuce%20field&image_size=landscape_16_9',
            description: '采摘活动是一项亲子互动的好选择，让您和家人一起体验采摘的乐趣，感受田园风光。农场提供多种时令蔬菜水果供您采摘，新鲜美味，健康营养。',
            location: '农场优选生态农场',
            people: '不限',
            content: '1. 采摘体验：根据季节采摘不同的蔬菜水果\n2. 农场参观：了解农场的种植过程\n3. 农产品购买：购买新鲜的农场产品\n4. 休闲野餐：在农场指定区域进行野餐',
            images: [
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20lettuce%20field&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=people%20picking%20vegetables&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=farm%20fresh%20produce&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=family%20picnic%20on%20farm&image_size=square'
            ]
          };
          break;
        case '3':
          activityData = {
            id: activityId,
            title: '草莓采摘体验',
            price: '门票: 30 ¥/人',
            date: '2025.3.1-2025.4.30',
            image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=strawberry%20picking&image_size=landscape_16_9',
            description: '草莓采摘体验是春季最受欢迎的活动之一，新鲜的草莓酸甜可口，营养丰富。在草莓大棚里，您可以亲手采摘最新鲜的草莓，感受春天的气息。',
            location: '农场优选草莓基地',
            people: '不限',
            content: '1. 草莓采摘：在草莓大棚里采摘新鲜草莓\n2. 草莓品尝：免费品尝新鲜草莓\n3. 草莓购买：购买新鲜草莓带回家\n4. 亲子活动：草莓DIY制作',
            images: [
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=strawberry%20picking&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20strawberries&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=children%20picking%20strawberries&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=strawberry%20diy&image_size=square'
            ]
          };
          break;
        case '4':
          activityData = {
            id: activityId,
            title: '葡萄采摘节',
            price: '门票: 50 ¥/人',
            date: '2025.7.1-2025.8.31',
            image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=grape%20picking&image_size=landscape_16_9',
            description: '葡萄采摘节是夏季的重头戏，农场种植了多种品种的葡萄，包括巨峰、玫瑰香、阳光玫瑰等。在葡萄架下，您可以品尝到最新鲜的葡萄，感受夏日的清凉。',
            location: '农场优选葡萄基地',
            people: '不限',
            content: '1. 葡萄采摘：在葡萄架下采摘新鲜葡萄\n2. 葡萄品尝：免费品尝不同品种的葡萄\n3. 葡萄购买：购买新鲜葡萄带回家\n4. 葡萄酒品鉴：品尝农场自酿葡萄酒',
            images: [
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=grape%20picking&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20grapes&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=grape%20vineyard&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=wine%20tasting&image_size=square'
            ]
          };
          break;
        case '5':
          activityData = {
            id: activityId,
            title: '农场露营体验',
            price: '费用: 120 ¥/晚',
            date: '2025.4.1-2025.10.31',
            image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=farm%20camping%20tent&image_size=landscape_16_9',
            description: '农场露营体验是一种亲近自然的方式，在农场的草地上搭建帐篷，夜晚欣赏星空，清晨听着鸟鸣醒来。农场提供帐篷租赁服务，也可以自带帐篷。',
            location: '农场优选露营基地',
            people: '限20人',
            content: '1. 帐篷搭建：学习如何搭建帐篷\n2. 篝火晚会：夜晚举行篝火晚会\n3. 星空观测：使用望远镜观测星空\n4. 农场早餐：品尝农场自制早餐\n5. 晨露采摘：清晨采摘带有晨露的蔬菜',
            images: [
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=farm%20camping%20tent&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=campfire%20at%20night&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=stargazing&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=farm%20breakfast&image_size=square'
            ]
          };
          break;
        case '6':
          activityData = {
            id: activityId,
            title: '篝火露营晚会',
            price: '费用: 180 ¥/人',
            date: '2025.5.1-2025.9.30',
            image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=camping%20with%20campfire&image_size=landscape_16_9',
            description: '篝火露营晚会是一种充满乐趣的户外活动，在篝火旁唱歌跳舞，品尝烤串，度过一个难忘的夜晚。农场提供全套露营装备和食材，让您的露营体验更加舒适。',
            location: '农场优选露营基地',
            people: '限30人',
            content: '1. 帐篷搭建：由专业人员指导搭建帐篷\n2. 篝火晚会：举行篝火晚会，唱歌跳舞\n3. 烧烤盛宴：品尝农场特色烧烤\n4. 星空观测：使用专业望远镜观测星空\n5. 农场早餐：品尝农场自制早餐\n6. 农场参观：参观农场的种植养殖区域',
            images: [
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=camping%20with%20campfire&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=bbq%20party&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=people%20singing%20around%20campfire&image_size=square',
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=farm%20camping%20morning&image_size=square'
            ]
          };
          break;
        default:
          activityData = {
            id: activityId,
            title: '活动详情',
            price: '免费',
            date: '待定',
            image: 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=farm%20activity&image_size=landscape_16_9',
            description: '活动详情加载中...',
            location: '农场优选',
            people: '不限',
            content: '活动内容待定',
            images: [
              'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=farm%20activity&image_size=square'
            ]
          };
      }
      
      // 更新数据
      this.setData({
        activity: activityData,
        loading: false
      });
      
      wx.hideLoading();
    }, 1000);
  },

  // 联系客服
  contactService: function() {
    wx.showToast({
      title: '联系客服功能开发中',
      icon: 'none'
    });
  },

  // 立即报名
  registerActivity: function() {
    wx.showToast({
      title: '报名功能开发中',
      icon: 'none'
    });
  },

  // 预览图片
  previewImage: function(e) {
    const index = e.currentTarget.dataset.index;
    const images = this.data.activity.images;
    wx.previewImage({
      current: images[index],
      urls: images,
      success: function(res) {
        console.log('预览图片成功');
      },
      fail: function(res) {
        console.log('预览图片失败', res);
      }
    });
  }
});