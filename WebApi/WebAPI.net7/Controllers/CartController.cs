using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// 购物车接口示例：当前未设计专门 cart 表，临时使用商品数据生成演示购物车。
        /// 后续如新增购物车表，可在此处改为真实持久化。
        /// </summary>
        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /api/cart/list
        [HttpGet("list")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CartItemDto>>>> List()
        {
            var goods = await _db.Commodities
                .AsNoTracking()
                .OrderByDescending(c => c.CommodityId)
                .Take(3)
                .Select(c => new CartItemDto
                {
                    Id = Guid.Empty,
                    Quantity = 1,
                    Goods = new GoodsDto
                    {
                        Id = Guid.Empty,
                        Name = c.ProductName,
                        Price = 0m,
                        ImageUrl = Url.Action("GetImage", "Commodity", new { id = c.CommodityId })
                    }
                })
                .ToListAsync();

            return ApiResponse<IEnumerable<CartItemDto>>.Ok(goods);
        }

        // POST /api/cart/add
        [HttpPost("add")]
        public ActionResult<ApiResponse<object>> Add([FromBody] object body)
        {
            // 购物车表尚未落库，这里仅返回成功以便前端联调
            return ApiResponse<object>.Ok(null);
        }

        // PUT /api/cart/update
        [HttpPut("update")]
        public ActionResult<ApiResponse<object>> Update([FromBody] object body)
        {
            return ApiResponse<object>.Ok(null);
        }

        // DELETE /api/cart/delete
        [HttpDelete("delete")]
        public ActionResult<ApiResponse<object>> Delete([FromBody] object body)
        {
            return ApiResponse<object>.Ok(null);
        }

        // DELETE /api/cart/clear
        [HttpDelete("clear")]
        public ActionResult<ApiResponse<object>> Clear()
        {
            return ApiResponse<object>.Ok(null);
        }
    }
}
