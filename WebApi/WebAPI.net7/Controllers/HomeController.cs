using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// 首页接口模板：从数据库中读取数据并转换为小程序所需结构。
        /// swiperList、functionButtons 目前仍为演示数据，farmGoods / hotDishes 从 commodity 等表查询。
        /// </summary>
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET /api/home/index
        /// 返回首页轮播图、功能按钮、农场优选商品、热销菜品等数据。
        /// </summary>
        [HttpGet("index")]
        public async Task<ActionResult<ApiResponse<object>>> Index()
        {
            // 轮播图与功能按钮：暂时写死，方便前端联调；后续可拆表维护
            var swiperList = new[]
            {
                new { id = 1, image = "/images/banner1.jpg" },
                new { id = 2, image = "/images/banner2.jpg" }
            };

            var functionButtons = new[]
            {
                new { id = 1, name = "认购一亩田", color = "#4CAF50", path = "/pages/activity/activity" },
                new { id = 2, name = "农场优选", color = "#FF9800", path = "/pages/farm-goods/farm-goods" },
                new { id = 3, name = "点餐", color = "#F44336", path = "/pages/order/order" },
                new { id = 4, name = "活动中心", color = "#2196F3", path = "/pages/activity/activity" }
            };

            // 农场优选商品：从 commodity 表读取前若干条作为推荐数据
            var farmGoodsQuery = _db.Commodities
                .AsNoTracking()
                .OrderByDescending(c => c.CommodityId)
                .Take(8);

            var farmGoods = await farmGoodsQuery
                .Select(c => new
                {
                    id = c.CommodityId,
                    name = c.ProductName,
                    image = Url.Action("GetImage", "Commodity", new { id = c.CommodityId }),
                    price = 0m,
                    originalPrice = 0m,
                    tags = Array.Empty<string>(),
                    stock = c.InStock ?? 0
                })
                .ToListAsync();

            // 热销菜品：当前没有 dish 的实体映射，这里复用商品数据作为“热销菜品”示例
            var hotDishes = farmGoods
                .Take(4)
                .Select(x => new
                {
                    id = x.id,
                    name = x.name,
                    image = x.image,
                    price = x.price,
                    tags = new[] { "热销推荐" }
                })
                .ToList();

            var data = new
            {
                swiperList,
                functionButtons,
                farmGoods,
                hotDishes
            };

            return ApiResponse<object>.Ok(data);
        }
    }
}