using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoodsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// 商品相关接口：列表、详情、首页推荐等，全部从商品表读取。
        /// </summary>
        public GoodsController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET /api/goods
        /// 商品分页列表，可选分类和关键字过滤。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<GoodsDto>>>> GetList(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? keyword = null)
        {
            var query = _db.Commodities.AsNoTracking().AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c => c.ProductName.Contains(keyword));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.CommodityId)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new GoodsDto
                {
                    Id = Guid.Empty,
                    Name = c.ProductName,
                    Price = 0m
                })
                .ToListAsync();

            var paged = new PagedResult<GoodsDto>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Total = total,
                Items = items
            };

            return ApiResponse<PagedResult<GoodsDto>>.Ok(paged);
        }

        /// <summary>
        /// GET /api/goods/{id}
        /// demo 使用的商品详情接口，这里从数据库按主键读取基本信息。
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<GoodsDto>>> Get(int id)
        {
            var c = await _db.Commodities.AsNoTracking().FirstOrDefaultAsync(x => x.CommodityId == id);
            if (c == null)
            {
                return ApiResponse<GoodsDto>.Fail("商品不存在", 404);
            }

            var dto = new GoodsDto
            {
                Id = Guid.Empty,
                Name = c.ProductName,
                Price = 0m
            };

            return ApiResponse<GoodsDto>.Ok(dto);
        }

        /// <summary>
        /// GET /api/goods/detail?goodsId=xxx
        /// 按文档返回完整商品详情结构。
        /// </summary>
        [HttpGet("detail")]
        public async Task<ActionResult<ApiResponse<object>>> Detail([FromQuery] int goodsId)
        {
            var c = await _db.Commodities.AsNoTracking().FirstOrDefaultAsync(x => x.CommodityId == goodsId);
            if (c == null)
            {
                return ApiResponse<object>.Fail("商品不存在", 404);
            }

            var data = new
            {
                id = c.CommodityId,
                name = c.ProductName,
                price = 0m,
                image = Url.Action("GetImage", "Commodity", new { id = c.CommodityId }),
                detailImage = Url.Action("GetImage", "Commodity", new { id = c.CommodityId }),
                description = c.SpecDescription ?? "",
                weight = "",
                storage = "",
                stock = c.InStock ?? 0,
                tags = Array.Empty<string>()
            };

            return ApiResponse<object>.Ok(data);
        }

        /// <summary>
        /// GET /api/goods/recommend
        /// 首页推荐商品列表，从商品表中取前几条。
        /// </summary>
        [HttpGet("recommend")]
        public async Task<ActionResult<ApiResponse<IEnumerable<GoodsDto>>>> Recommend()
        {
            var list = await _db.Commodities
                .AsNoTracking()
                .OrderByDescending(c => c.CommodityId)
                .Take(4)
                .Select(c => new GoodsDto
                {
                    Id = Guid.Empty,
                    Name = c.ProductName,
                    Price = 0m
                })
                .ToListAsync();

            return ApiResponse<IEnumerable<GoodsDto>>.Ok(list);
        }
    }
}
