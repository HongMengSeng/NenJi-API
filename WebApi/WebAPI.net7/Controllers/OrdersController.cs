using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// 订单相关接口：创建订单、列表、详情、取消、确认收货。
        /// </summary>
        public OrdersController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Used by: demo/pages/cart/cart.js (生成订单) and demo/pages/order/order.js (下单)
        #region Create - POST /api/order/create
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreateOrderRequest body)
        {
            if (body == null || body.AddressId <= 0 || body.TotalPrice <= 0)
            {
                return ApiResponse<object>.Fail("参数错误", 400);
            }

            // 当前没有登录态，简单使用第一条用户记录
            var user = await _db.Users.OrderBy(u => u.UserId).FirstOrDefaultAsync();
            if (user == null)
            {
                return ApiResponse<object>.Fail("用户不存在", 404);
            }

            var now = DateTime.UtcNow;
            var orderNumber = now.ToString("yyyyMMddHHmmss") + user.UserId;

            var entity = new Models.Entities.OrderMain
            {
                OrderNumber = orderNumber,
                UserId = user.UserId,
                ActualPayment = body.TotalPrice,
                AddressId = body.AddressId,
                OrderType = 1,
                TotalAmount = body.TotalPrice,
                OrderStatus = 0,
                PaymentStatus = 0,
                DeliveryMethods = 1,
                OrderCreateTime = now,
                SnapshotUserNickname = user.WxNickname ?? user.UserNo
            };

            _db.Orders.Add(entity);
            await _db.SaveChangesAsync();

            return ApiResponse<object>.Ok(new { orderId = entity.OrderId });
        }
        #endregion

        // Used by: demo/pages/order/order.js (订单列表)
        #region GetList - GET /api/order/list
        [HttpGet("list")]
        public async Task<ActionResult<ApiResponse<object>>> GetList(
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _db.Orders.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                // 简单示例：将非空状态统一映射为订单状态 = 0 以外的记录
                query = query.Where(o => o.OrderStatus != 0);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.OrderCreateTime ?? DateTime.MinValue)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    id = o.OrderId,
                    orderNo = o.OrderNumber,
                    totalPrice = o.TotalAmount ?? 0m,
                    status = (o.OrderStatus ?? 0).ToString(),
                    createTime = o.OrderCreateTime.HasValue
     ? o.OrderCreateTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
     : ""
                })
                .ToListAsync();

            var data = new
            {
                orderList = items,
                total,
                page,
                pageSize
            };

            return ApiResponse<object>.Ok(data);
        }
        #endregion

        // Used by: demo/pages/order/order.js (订单详情)
        #region Get - GET /api/order/detail
        [HttpGet("detail")]
        public async Task<ActionResult<ApiResponse<object>>> Detail([FromQuery] long orderId)
        {
            var o = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == orderId);
            if (o == null)
            {
                return ApiResponse<object>.Fail("订单不存在", 404);
            }

            var address = new
            {
                name = o.SnapshotReceiverName ?? "",
                phone = o.SnapshotReceiverPhone ?? "",
                address = o.SnapshotDeliveryAddress ?? o.ShippingAddress ?? ""
            };

            var data = new
            {
                id = o.OrderId,
                orderNo = o.OrderNumber,
                totalPrice = o.TotalAmount ?? 0m,
                status = (o.OrderStatus ?? 0).ToString(),
                createTime = o.OrderCreateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                address,
                goodsList = Array.Empty<object>()
            };

            return ApiResponse<object>.Ok(data);
        }
        #endregion

        // Used by: demo/pages/order/order.js (取消订单)
        #region Cancel - PUT /api/order/cancel
        [HttpPut("cancel")]
        public async Task<ActionResult<ApiResponse<object>>> Cancel([FromBody] CancelOrderRequest body)
        {
            var entity = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == body.OrderId);
            if (entity == null)
            {
                return ApiResponse<object>.Fail("订单不存在", 404);
            }

            entity.OrderStatus = 4; // 示例：4 表示已取消
            await _db.SaveChangesAsync();

            return ApiResponse<object>.Ok(null);
        }
        #endregion

        // Used by: demo/pages/order/order.js (确认收货)
        #region Confirm - demo/pages/order/order.js确认收货
        [HttpPost("{id}/confirm")]
        public async Task<ActionResult<ApiResponse<object>>> Confirm(long id)
        {
            var entity = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
            if (entity == null)
            {
                return ApiResponse<object>.Fail("订单不存在", 404);
            }

            entity.OrderStatus = 3; // 示例：3 表示已完成
            await _db.SaveChangesAsync();

            return ApiResponse<object>.Ok(null);
        }
        #endregion

        public class CreateOrderRequest
        {
            public int AddressId { get; set; }
            public decimal TotalPrice { get; set; }
            public string? Remark { get; set; }
            public IEnumerable<int>? CartIds { get; set; }
        }

        public class CancelOrderRequest
        {
            public long OrderId { get; set; }
        }
    }
}
