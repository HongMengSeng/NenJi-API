using System.Linq;

using Microsoft.AspNetCore.Mvc;

using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoApiController : ControllerBase
{
    [HttpGet("home")]
    public ActionResult<ApiResponse<object>> GetHome([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;

       var allFarmGoods = new object[]
        {
            new { id = 1, name = "甜养玉米500g", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20sweet%20corn&image_size=square", price = 8.9, originalPrice = 9.9, tags = new[] { "软糯香甜", "颗粒饱满" }, stock = 464646 },
            new { id = 2, name = "农家土豆500g", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20potatoes&image_size=square", price = 8.9, originalPrice = 9.9, tags = new[] { "新鲜采摘", "农场直供" }, stock = 464646 },
            new { id = 3, name = "时令水果500g", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20apples%20and%20oranges&image_size=square", price = 8.9, originalPrice = 9.9, tags = new[] { "香甜多汁", "现摘现发" }, stock = 464646 },
            new { id = 4, name = "农家番茄500g", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20tomatoes&image_size=square", price = 8.9, originalPrice = 9.9, tags = new[] { "自然成熟", "口感鲜甜" }, stock = 464646 },
           
        };

        var allHotDishes = new object[]
        {
            new { id = 1, name = "剁椒鱼头", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=spicy%20fish%20head%20dish&image_size=square", price = 68.9, tags = new[] { "月销10000份" } },
            new { id = 2, name = "农家小炒肉", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=stir%20fried%20pork%20with%20pepper&image_size=square", price = 38.9, tags = new[] { "招牌热销" } },
            new { id = 3, name = "酸菜鱼", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=pickled%20fish%20dish&image_size=square", price = 58.9, tags = new[] { "人气推荐" } },
            new { id = 4, name = "辣子鸡", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=spicy%20chicken%20dish&image_size=square", price = 42.9, tags = new[] { "下饭必点" } },
        };

        var farmGoods = allFarmGoods
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        var hotDishes = allHotDishes
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        var data = new
        {
            swiperList = page == 1
                ? new[]
                {
                    new { id = 1, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" },
                    new { id = 2, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" },
                    new { id = 3, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" }
                }
                : Array.Empty<object>(),

            functionButtons = page == 1
                ? new[]
                {
                    new { id = 1, name = "认购一亩田", color = "#4CAF50", path = "/pages/acre/acre" },
                    new { id = 2, name = "农场优选", color = "#FF9800", path = "/pages/farm-goods/farm-goods" },
                    new { id = 3, name = "点餐", color = "#F44336", path = "/pages/order/order" },
                    new { id = 4, name = "活动中心", color = "#2196F3", path = "/pages/activity/activity" }
                }
                : Array.Empty<object>(),

            acreProjects = page == 1
                ? new[]
                {
                    new
                    {
                        id = 1,
                        name = "认购一亩田",
                        description = "新型农场推出的共享农业体验项目。",
                        price = 99999,
                        image = "https://img.freepik.com/free-photo/yellow-field-with-lines_1127-3388.jpg"
                    },
                    new
                    {
                        id = 2,
                        name = "某某农场",
                        description = "新型农场推出的共享农业体验项目。",
                        price = 88888,
                        image = "https://img.freepik.com/free-photo/agriculture-field-with-growing-crops_23-2148872538.jpg"
                    }
                }
                : Array.Empty<object>(),

            farmGoods = farmGoods,
            hotDishes = hotDishes,
            hasMore = (page * pageSize) < Math.Max(allFarmGoods.Length, allHotDishes.Length)
        };

        return ApiResponse<object>.Ok(data);
    }


    [HttpGet("goods")]
    public ActionResult<ApiResponse<object>> GetGoods(
     [FromQuery] string category = "all",
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 6)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 6 : pageSize;

        var swiperList = new object[]
        {
        new
        {
            id = 1,
            image = "https://img2.baidu.com/it/u=1977433049,53820872&fm=253&fmt=auto&app=138&f=JPEG?w=342&h=608"
        },
        new
        {
            id = 2,
            image = "https://img2.baidu.com/it/u=1977433049,53820872&fm=253&fmt=auto&app=138&f=JPEG?w=342&h=608"
        },
        new
        {
            id = 3,
            image = "https://img2.baidu.com/it/u=1977433049,53820872&fm=253&fmt=auto&app=138&f=JPEG?w=342&h=608"
        }
        };

        var categories = new object[]
        {
        new { id = "all", name = "全部商品", color = "#4CAF50", icon = "全" },
        new { id = "new", name = "新品上市", color = "#FF9800", icon = "新" }
        };

        var categoryGoods = new Dictionary<string, object[]>
        {
            ["all"] = new object[]
            {
            new { id = 1, name = "白糯玉米 800g", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20corn%20on%20the%20cob&image_size=square", price = 9.4, tags = new[] { "农场直供", "新鲜采摘" }, stock = 14515415145,originalPrice = 10 },
            new { id = 2, name = "牢大 800g", image = "https://img2.baidu.com/it/u=1977433049,53820872&fm=253&fmt=auto&app=138&f=JPEG?w=342&h=608", price = 9.4, tags = new[] { "农场直供", "新鲜采摘" }, stock = 14515415145,originalPrice = 10 },
            new { id = 3, name = "牢大 800g", image = "https://img2.baidu.com/it/u=1977433049,53820872&fm=253&fmt=auto&app=138&f=JPEG?w=342&h=608", price = 9.4, tags = new[] { "农场直供", "新鲜采摘" }, stock = 14515415145,originalPrice = 10 },
            new { id = 4, name = "牢大 800g", image = "https://img2.baidu.com/it/u=1977433049,53820872&fm=253&fmt=auto&app=138&f=JPEG?w=342&h=608", price = 9.4, tags = new[] { "农场直供", "新鲜采摘" }, stock = 14515415145,originalPrice = 10 },
            new { id = 5, name = "牢大 800g", image = "https://img2.baidu.com/it/u=1977433049,53820872&fm=253&fmt=auto&app=138&f=JPEG?w=342&h=608", price = 9.4, tags = new[] { "农场直供", "新鲜采摘" }, stock = 14515415145 ,originalPrice = 10},
            new { id = 6, name = "白糯玉米 800g", image = "https://img2.baidu.com/it/u=1977433049,53820872&fm=253&fmt=auto&app=138&f=JPEG?w=342&h=608", price = 9.4, tags = new[] { "农场直供", "新鲜采摘" }, stock = 14515415145,originalPrice = 10 },
            new { id = 7, name = "白糯玉米 800g", image = "https://img2.baidu.com/it/u=1977433049,53820872&fm=253&fmt=auto&app=138&f=JPEG?w=342&h=608", price = 9.4, tags = new[] { "农场直供", "新鲜采摘" }, stock = 14515415145 ,originalPrice = 10},
            new { id = 8, name = "白糯玉米 800g", image = "https://img2.baidu.com/it/u=1977433049,53820872&fm=253&fmt=auto&app=138&f=JPEG?w=342&h=608", price = 9.4, tags = new[] { "农场直供", "新鲜采摘" }, stock = 14515415145,originalPrice = 10 },
            new { id = 9, name = "白糯玉米 800g", image = "https://img2.baidu.com/it/u=1977433049,53820872&fm=253&fmt=auto&app=138&f=JPEG?w=342&h=608", price = 9.4, tags = new[] { "农场直供", "新鲜采摘" }, stock = 14515415145 ,originalPrice = 10},
            new { id = 10, name = "白糯玉米 800g", image = "https://img2.baidu.com/it/u=1977433049,53820872&fm=253&fmt=auto&app=138&f=JPEG?w=342&h=608", price = 9.4, tags = new[] { "农场直供", "新鲜采摘" }, stock = 14515415145,originalPrice = 10 }
            },
            ["new"] = new object[]
            {
            new { id = 7, name = "新品玉米 800g", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=new%20corn%20product&image_size=square", price = 10.4, tags = new[] { "农场直供", "新品上市" }, stock = 9999 }
            }
        };

        if (!categoryGoods.TryGetValue(category, out var allGoods))
        {
            allGoods = categoryGoods["all"];
            category = "all";
        }

        var total = allGoods.Length;
        var items = allGoods
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        var hasMore = page * pageSize < total;

        return ApiResponse<object>.Ok(new
        {
            swiperList = page == 1 ? swiperList : Array.Empty<object>(),
            categories = page == 1 ? categories : Array.Empty<object>(),
            category,
            items,
            page,
            pageSize,
            total,
            hasMore
        });
    }


    [HttpGet("goods/{id}")]
    public ActionResult<ApiResponse<object>> GetGoodsById(int id)
    {
        var item = id switch
        {
            1 => new
            {
                id = 1,
                name = "有机生菜",
                price = 30,
                image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20organic%20lettuce&image_size=square",
                detailImage = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=lettuce%20field&image_size=portrait_4_3",
                description = "有机生菜，无农药残留。",
                weight = "500g",
                storage = "冷藏"
            },
            2 => new
            {
                id = 2,
                name = "农家西红柿",
                price = 30,
                image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20tomatoes&image_size=square",
                detailImage = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=tomato%20field&image_size=portrait_4_3",
                description = "农家种植的西红柿。",
                weight = "500g",
                storage = "常温"
            },
            _ => new
            {
                id = 3,
                name = "直升机",
                price = 30,
                image = "https://img2.baidu.com/it/u=1977433049,53820872&fm=253&fmt=auto&app=138&f=JPEG?w=342&h=608",
                detailImage = "https://img2.baidu.com/it/u=1977433049,53820872&fm=253&fmt=auto&app=138&f=JPEG?w=342&h=608",
                description = "默认商品。",
                weight = "500g",
                storage = "冷藏"
            }
        };

        return ApiResponse<object>.Ok(item);
    }

    [HttpGet("acres")]
    public ActionResult<ApiResponse<object>> GetAcres()
    {
        var swiperList = new[]
        {
            new { id = 1, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" },
            new { id = 2, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" },
            new { id = 3, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" }
        };

        var list = new[]
        {
            new { id = 1, name = "xxx田地", description = "认购一亩田...", price = "¥99999", image = "https://img.freepik.com/free-photo/yellow-field-with-lines_1127-3388.jpg" },
            new { id = 2, name = "xxx田地", description = "认购一亩田...", price = "¥99999", image = "https://img.freepik.com/free-photo/agriculture-field-with-growing-crops_23-2148872538.jpg" },
            new { id = 3, name = "xxx田地", description = "认购一亩田...", price = "¥99999", image = "https://img.freepik.com/free-photo/wheat-field_1127-3185.jpg" }
        };

        return ApiResponse<object>.Ok(new
        {
            swiperList,
            list,
            items = list
        });
    }

    [HttpGet("acres/{id}")]
    public ActionResult<ApiResponse<object>> GetAcreById(int id)
    {
        var details = new Dictionary<int, object>
        {
            [1] = new
            {
                id = 1,
                name = "xxx田地",
                price = "99999元",
                image = "https://img.freepik.com/free-photo/yellow-field-with-lines_1127-3388.jpg",
                description = "本地块为标准型农业用地，适合认购体验。",
                swiperList = new[]
                {
                    new { id = 1, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" },
                    new { id = 2, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" },
                    new { id = 3, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" }
                }
            },
            [2] = new
            {
                id = 2,
                name = "xxx田地",
                price = "99999元",
                image = "https://img.freepik.com/free-photo/agriculture-field-with-growing-crops_23-2148872538.jpg",
                description = "本地块为标准型农业用地，适合认购体验。",
                swiperList = new[]
                {
                    new { id = 1, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" },
                    new { id = 2, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" },
                    new { id = 3, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" }
                }
            },
            [3] = new
            {
                id = 3,
                name = "xxx田地",
                price = "99999元",
                image = "https://img.freepik.com/free-photo/wheat-field_1127-3185.jpg",
                description = "本地块为标准型农业用地，适合认购体验。",
                swiperList = new[]
                {
                    new { id = 1, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" },
                    new { id = 2, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" },
                    new { id = 3, image = "https://img0.baidu.com/it/u=3670860447,3495259318&fm=253&fmt=auto&app=120&f=JPEG?w=667&h=500" }
                }
            }
        };

        if (!details.TryGetValue(id, out var acre))
        {
            acre = details[1];
        }

        return ApiResponse<object>.Ok(acre);
    }

    [HttpPost("acres/{id}/adopt")]
    public ActionResult<ApiResponse<object>> DemoAdopt(int id, [FromBody] object body)
    {
        return ApiResponse<object>.Ok(new { id, adopted = true });
    }

    [HttpGet("activities")]
    public ActionResult<ApiResponse<object>> GetActivities()
    {
        var activities = new[]
        {
            new { id = 1, title = "农家研学活动报名中", price = "门票: 10-20 元", date = "2025.2.25-2025.3.6", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=children%20playing%20football%20on%20farm&image_size=landscape_16_9" },
            new { id = 2, title = "采摘活动报名中", price = "门票: 10-50 元", date = "2025.2.25-2025.3.6", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20lettuce%20field&image_size=landscape_16_9" }
        };

        return ApiResponse<object>.Ok(activities);
    }

    [HttpGet("cart")]
    public ActionResult<ApiResponse<object>> GetCart()
    {
        var cart = new
        {
            cartList = new[]
            {
                new { id = 1, name = "白糯玉米 800g", image = "/images/activity-active.png", tag = "包邮", price = 69.99, count = 1, @checked = false },
                new { id = 2, name = "黄花鱼", image = "/images/activity-active.png", tag = "顺丰包邮", price = 169.00, count = 1, @checked = false }
            },
            totalPrice = "0.00"
        };

        return ApiResponse<object>.Ok(cart);
    }

    [HttpPost("cart/items")]
    public ActionResult<ApiResponse<object>> AddCartItem([FromBody] object body)
    {
        return ApiResponse<object>.Ok(new { id = 1 });
    }

    [HttpPut("cart/items/{id}")]
    public ActionResult<ApiResponse<object>> UpdateCartItem(int id, [FromBody] object body)
    {
        return ApiResponse<object>.Ok(null);
    }

    [HttpDelete("cart/items/{id}")]
    public ActionResult<ApiResponse<object>> DeleteCartItem(int id)
    {
        return ApiResponse<object>.Ok(null);
    }

    [HttpDelete("cart")]
    public ActionResult<ApiResponse<object>> ClearCart()
    {
        return ApiResponse<object>.Ok(null);
    }
    [HttpGet("orders")]
    public ActionResult<ApiResponse<object>> GetOrders(
      [FromQuery] string categoryId = "vegetables",
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 6)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 6 : pageSize;

        var categories = new[]
        {
        new { id = "vegetables", name = "新鲜蔬菜" },
        new { id = "meat", name = "肉类产品" },
        new { id = "eggs", name = "禽蛋产品" },
        new { id = "dairy", name = "乳制品" },
        new { id = "staple", name = "主食" }
    };

        var goodsMap = new Dictionary<string, object[]>
        {
            ["vegetables"] = new object[]
            {
            new { id = 1, name = "有机生菜", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20organic%20lettuce&image_size=square", price = 30, sold = 150, stock = 30 },
            new { id = 2, name = "农家西红柿", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20tomatoes&image_size=square", price = 30, sold = 200, stock = 30 },
            new { id = 3, name = "新鲜黄瓜", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20cucumbers&image_size=square", price = 30, sold = 180, stock = 30 },
            new { id = 4, name = "紫甘蓝", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20purple%20cabbage&image_size=square", price = 30, sold = 160, stock = 30 },
            new { id = 5, name = "胡萝卜", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20carrots&image_size=square", price = 30, sold = 170, stock = 30 },
            new { id = 6, name = "青椒", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20green%20pepper&image_size=square", price = 30, sold = 140, stock = 30 },
            new { id = 7, name = "白萝卜", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20white%20radish&image_size=square", price = 30, sold = 120, stock = 30 }
            },
            ["meat"] = new object[]
            {
            new { id = 8, name = "土猪肉", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20pork%20meat&image_size=square", price = 30, sold = 100, stock = 30 },
            new { id = 9, name = "农家土鸡", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20chicken&image_size=square", price = 30, sold = 80, stock = 30 },
            new { id = 10, name = "鲜牛肉", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20beef&image_size=square", price = 30, sold = 95, stock = 30 },
            new { id = 11, name = "羊排", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20lamb%20chops&image_size=square", price = 30, sold = 70, stock = 30 },
            new { id = 12, name = "五花肉", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20pork%20belly&image_size=square", price = 30, sold = 110, stock = 30 },
            new { id = 13, name = "鸡翅中", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20chicken%20wings&image_size=square", price = 30, sold = 85, stock = 30 },
            new { id = 14, name = "排骨", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20pork%20ribs&image_size=square", price = 30, sold = 90, stock = 30 }
            },
            ["eggs"] = new object[]
            {
            new { id = 15, name = "土鸡蛋", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20eggs&image_size=square", price = 30, sold = 300, stock = 30 },
            new { id = 16, name = "鸭蛋", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20duck%20eggs&image_size=square", price = 30, sold = 150, stock = 30 },
            new { id = 17, name = "鹅蛋", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20goose%20eggs&image_size=square", price = 30, sold = 50, stock = 30 },
            new { id = 18, name = "鹌鹑蛋", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20quail%20eggs&image_size=square", price = 30, sold = 80, stock = 30 },
            new { id = 19, name = "咸鸭蛋", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=salted%20duck%20eggs&image_size=square", price = 30, sold = 90, stock = 30 },
            new { id = 20, name = "柴鸡蛋", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=free%20range%20eggs&image_size=square", price = 30, sold = 100, stock = 30 },
            new { id = 21, name = "双黄蛋", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=double%20yolk%20eggs&image_size=square", price = 30, sold = 60, stock = 30 },
            new { id = 22, name = "乌鸡蛋", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=black%20chicken%20eggs&image_size=square", price = 30, sold = 55, stock = 30 },
            new { id = 23, name = "鸽子蛋", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=pigeon%20eggs&image_size=square", price = 30, sold = 40, stock = 30 }
            },
            ["dairy"] = new object[]
            {
            new { id = 24, name = "新鲜牛奶", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20milk&image_size=square", price = 30, sold = 200, stock = 30 },
            new { id = 25, name = "农家酸奶", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=homemade%20yogurt&image_size=square", price = 30, sold = 180, stock = 30 },
            new { id = 26, name = "纯牛奶", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=pure%20milk&image_size=square", price = 30, sold = 150, stock = 30 },
            new { id = 27, name = "高钙牛奶", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=high%20calcium%20milk&image_size=square", price = 30, sold = 140, stock = 30 },
            new { id = 28, name = "奶酪", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=cheese&image_size=square", price = 30, sold = 90, stock = 30 },
            new { id = 29, name = "黄油", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=butter&image_size=square", price = 30, sold = 85, stock = 30 },
            new { id = 30, name = "奶昔", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=milkshake&image_size=square", price = 30, sold = 75, stock = 30 },
            new { id = 31, name = "儿童牛奶", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=kids%20milk&image_size=square", price = 30, sold = 95, stock = 30 },
            new { id = 32, name = "鲜奶布丁", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=milk%20pudding&image_size=square", price = 30, sold = 65, stock = 30 }
            },
            ["staple"] = new object[]
            {
            new { id = 33, name = "农家大米", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20rice&image_size=square", price = 30, sold = 250, stock = 30 },
            new { id = 34, name = "手工面条", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=homemade%20noodles&image_size=square", price = 30, sold = 150, stock = 30 },
            new { id = 35, name = "糯米", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=glutinous%20rice&image_size=square", price = 30, sold = 180, stock = 30 },
            new { id = 36, name = "小米", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=millet&image_size=square", price = 30, sold = 170, stock = 30 },
            new { id = 37, name = "玉米面", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=cornmeal&image_size=square", price = 30, sold = 130, stock = 30 },
            new { id = 38, name = "红薯粉", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=sweet%20potato%20flour&image_size=square", price = 30, sold = 120, stock = 30 },
            new { id = 39, name = "挂面", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=dried%20noodles&image_size=square", price = 30, sold = 140, stock = 30 },
            new { id = 40, name = "刀削面", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=knife%20cut%20noodles&image_size=square", price = 30, sold = 110, stock = 30 },
            new { id = 41, name = "荞麦面", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=buckwheat%20noodles&image_size=square", price = 30, sold = 100, stock = 30 },
            new { id = 42, name = "杂粮米", image = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=mixed%20grains%20rice&image_size=square", price = 30, sold = 160, stock = 30 }
            }
        };

        if (!goodsMap.ContainsKey(categoryId))
        {
            categoryId = "vegetables";
        }

        var allGoods = goodsMap[categoryId];
        var total = allGoods.Length;
        var items = allGoods
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        var hasMore = page * pageSize < total;

        var data = new
        {
            categories,
            currentCategory = categoryId,
            goodsList = items,
            page,
            pageSize,
            total,
            hasMore
        };

        return ApiResponse<object>.Ok(data);
    }


    [HttpPost("orders")]
    public ActionResult<ApiResponse<object>> CreateOrder([FromBody] object body)
    {
        return ApiResponse<object>.Ok(new { orderId = 1001 });
    }

    [HttpGet("profile")]
    public ActionResult<ApiResponse<UserDto>> GetDemoProfile()
    {
        var user = new UserDto { Id = Guid.NewGuid(), NickName = "游客", AvatarUrl = "", PhoneNumber = "" };
        return ApiResponse<UserDto>.Ok(user);
    }
}