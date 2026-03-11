using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FarmGoodsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// 农场优选页面数据，从分类和商品表读取并封装为小程序结构。
        /// </summary>
        public FarmGoodsController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET /api/farm-goods/index
        /// 返回轮播图、分类、今日优选、热门商品列表。
        /// </summary>
        [HttpGet("index")]
        public async Task<ActionResult<ApiResponse<object>>> Index()
        {
            var swiperList = new[]
            {
                new { id = 1, image = "/images/farm_banner1.jpg" },
                new { id = 2, image = "/images/farm_banner2.jpg" }
            };

            var categories = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.SortOrder ?? 0)
                .ThenBy(c => c.Id)
                .Select(c => new
                {
                    id = c.Id.ToString(),
                    name = c.CategoryName,
                    icon = "",
                    color = "#4CAF50"
                })
                .ToListAsync();

            var commodityQuery = _db.Commodities
                .AsNoTracking()
                .OrderByDescending(c => c.CommodityId);

            var todayGoods = await commodityQuery
                .Take(8)
                .Select(c => new
                {
                    id = c.CommodityId,
                    name = c.ProductName,
                    image = Url.Action("GetImage", "Commodity", new { id = c.CommodityId }),
                    price = 0m,
                    originalPrice = 0m,
                    stock = c.InStock ?? 0,
                    tags = Array.Empty<string>()
                })
                .ToListAsync();

            var hotGoods = await commodityQuery
                .Skip(0)
                .Take(8)
                .Select(c => new
                {
                    id = c.CommodityId,
                    name = c.ProductName,
                    image = Url.Action("GetImage", "Commodity", new { id = c.CommodityId }),
                    price = 0m,
                    originalPrice = 0m,
                    stock = c.InStock ?? 0,
                    tags = Array.Empty<string>()
                })
                .ToListAsync();

            var data = new
            {
                swiperList,
                categories,
                todayGoods,
                hotGoods
            };

            return ApiResponse<object>.Ok(data);
        }

        /// <summary>
        /// GET /api/farm-goods/category
        /// 根据分类 ID 分页查询商品列表。
        /// </summary>
        [HttpGet("category")]
        public async Task<ActionResult<ApiResponse<object>>> Category([FromQuery] string categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
            {
                return ApiResponse<object>.Fail("categoryId 必填", 400);
            }

            if (!int.TryParse(categoryId, out var cid))
            {
                return ApiResponse<object>.Fail("categoryId 必须为数字", 400);
            }

            var query = _db.Commodities
                .AsNoTracking()
                .Where(c => c.CategoryId == cid);

            var total = await query.CountAsync();

            var goodsList = await query
                .OrderByDescending(c => c.CommodityId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    id = c.CommodityId,
                    name = c.ProductName,
                    image = Url.Action("GetImage", "Commodity", new { id = c.CommodityId }),
                    price = 0m,
                    tags = Array.Empty<string>()
                })
                .ToListAsync();

            var data = new
            {
                goodsList,
                total,
                page,
                pageSize
            };

            return ApiResponse<object>.Ok(data);
        }

        /// <summary>
        /// GET /api/farm-goods/search
        /// 根据商品名称关键字搜索。
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<object>>> Search([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return ApiResponse<object>.Fail("keyword 必填", 400);
            }

            var query = _db.Commodities
                .AsNoTracking()
                .Where(c => c.ProductName.Contains(keyword));

            var total = await query.CountAsync();

            var goodsList = await query
                .OrderByDescending(c => c.CommodityId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    id = c.CommodityId,
                    name = c.ProductName,
                    image = Url.Action("GetImage", "Commodity", new { id = c.CommodityId }),
                    price = 0m,
                    tags = Array.Empty<string>()
                })
                .ToListAsync();

            var data = new
            {
                goodsList,
                total,
                page,
                pageSize
            };

            return ApiResponse<object>.Ok(data);
        }
    }
}