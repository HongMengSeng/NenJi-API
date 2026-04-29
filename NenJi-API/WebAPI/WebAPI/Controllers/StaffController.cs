using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using WebAPI.Common;
using WebAPI.Data;
using WebAPI.Entities;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/staff")]
public class StaffController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public StaffController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("today-stats")]
    public async Task<IActionResult> TodayStats(CancellationToken cancellationToken)
    {
        var staff = await GetCurrentStaffAsync(cancellationToken);
        if (staff is null)
        {
            return Ok(ApiResult.Fail("无权限，仅员工可访问", 403));
        }

        var start = DateTime.Today;
        var end = start.AddDays(1);
        var todayQuery = VerifiedOrdersQuery()
            .Where(x => x.PaymentTime >= start && x.PaymentTime < end);

        var todayVerified = await todayQuery.CountAsync(cancellationToken);
        var activityVerified = await todayQuery.CountAsync(x => x.OrderType == 4, cancellationToken);
        var pickingVerified = await todayQuery.CountAsync(x => x.OrderType == 3, cancellationToken);
        var pendingCount = await PendingVouchersQuery().CountAsync(cancellationToken);
        var lastVerifyTime = await todayQuery
            .OrderByDescending(x => x.PaymentTime)
            .Select(x => (DateTime?)x.PaymentTime)
            .FirstOrDefaultAsync(cancellationToken);

        return Ok(ApiResult.Success(new
        {
            todayVerified,
            pendingCount,
            activityVerified,
            pickingVerified,
            today_verify_count = todayVerified,
            last_verify_time = lastVerifyTime?.ToString("yyyy-MM-dd HH:mm:ss")
        }));
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyVoucherRequest? request, CancellationToken cancellationToken)
    {
        var staff = await GetCurrentStaffAsync(cancellationToken);
        if (staff is null)
        {
            return Ok(ApiResult.Fail("无权限，仅员工可执行核销", 403));
        }

        var code = NormalizeVoucherCode(request?.Code);
        if (string.IsNullOrWhiteSpace(code))
        {
            return Ok(ApiResult.Fail("券码不能为空", 400));
        }

        var order = await FindVoucherOrderAsync(code, true, cancellationToken);
        if (order is null)
        {
            return Ok(ApiResult.Fail("未找到该券码", 404));
        }

        if (!IsVoucherOrder(order))
        {
            return Ok(ApiResult.Fail("该订单不支持员工核销", 400));
        }

        if (order.OrderStatus == 3)
        {
            return Ok(ApiResult.Fail("该券已使用，无法重复核销", 409));
        }

        if (order.PaymentStatus != 1)
        {
            return Ok(ApiResult.Fail("该券未支付，无法核销", 403));
        }

        var expireTime = GetExpireTime(order);
        if (expireTime < DateTime.Now)
        {
            return Ok(ApiResult.Fail("该券已过期", 403));
        }

        order.OrderStatus = 3;
        order.PaymentTime = DateTime.Now;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == order.UserId, cancellationToken);

        var voucherType = MapVoucherType(order.OrderType);
        var title = MapVoucherTitle(order);
        var verifyTime = order.PaymentTime.ToString("yyyy-MM-dd HH:mm:ss");

        return Ok(ApiResult.Success(new
        {
            success = true,
            voucherId = order.OrderId.ToString(),
            voucherType,
            userName = ResolveUserName(user, order),
            userPhone = MaskPhone(user?.PhoneNumber ?? order.ContactNumber),
            content = title,
            verifyTime,
            voucher_id = order.OrderId.ToString(),
            voucher_type = voucherType,
            title,
            user_name = ResolveUserName(user, order),
            user_phone = MaskPhone(user?.PhoneNumber ?? order.ContactNumber),
            order_id = order.OrderId.ToString(),
            expire_time = expireTime.ToString("yyyy-MM-dd"),
            verify_time = verifyTime
        }, "核销成功"));
    }

    [HttpGet("vouchers")]
    public async Task<IActionResult> Vouchers(
        [FromQuery] string? type,
        [FromQuery] string? status = "unused",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var staff = await GetCurrentStaffAsync(cancellationToken);
        if (staff is null)
        {
            return Ok(ApiResult.Fail("无权限，仅员工可访问", 403));
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = VoucherOrdersQuery();
        query = ApplyVoucherTypeFilter(query, type);
        query = ApplyVoucherStatusFilter(query, status);

        var total = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(x => x.OrderCreationTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userMap = await LoadUserMapAsync(orders, cancellationToken);
        var list = orders.Select(order => BuildVoucherListItem(order, userMap)).ToList();

        return Ok(ApiResult.Success(new
        {
            total,
            page,
            pageSize,
            list
        }));
    }

    [HttpGet("verify-history")]
    public async Task<IActionResult> VerifyHistory(
        [FromQuery] bool today = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var staff = await GetCurrentStaffAsync(cancellationToken);
        if (staff is null)
        {
            return Ok(ApiResult.Fail("无权限，仅员工可访问", 403));
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = VerifiedOrdersQuery();
        if (today)
        {
            var start = DateTime.Today;
            query = query.Where(x => x.PaymentTime >= start && x.PaymentTime < start.AddDays(1));
        }
        else
        {
            if (DateTime.TryParse(startDate, out var start))
            {
                query = query.Where(x => x.PaymentTime >= start.Date);
            }

            if (DateTime.TryParse(endDate, out var end))
            {
                query = query.Where(x => x.PaymentTime < end.Date.AddDays(1));
            }
        }

        var total = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(x => x.PaymentTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userMap = await LoadUserMapAsync(orders, cancellationToken);
        var list = orders.Select(order => BuildHistoryItem(order, userMap, staff)).ToList();

        return Ok(ApiResult.Success(new
        {
            total,
            page,
            pageSize,
            list
        }));
    }

    private async Task<User?> GetCurrentStaffAsync(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("userId");
        if (!int.TryParse(userIdValue, out var userId) || userId <= 0)
        {
            return null;
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roleName = await _dbContext.Roles
            .AsNoTracking()
            .Where(x => x.RoleId == user.RoleId)
            .Select(x => x.RoleName)
            .FirstOrDefaultAsync(cancellationToken);

        return IsStaffRole(roleName) ? user : null;
    }

    private IQueryable<OrderEntity> VoucherOrdersQuery()
    {
        return _dbContext.Orders.AsNoTracking().Where(IsVoucherOrderExpression());
    }

    private IQueryable<OrderEntity> PendingVouchersQuery()
    {
        return VoucherOrdersQuery().Where(x => x.PaymentStatus == 1 && x.OrderStatus != 3 && x.OrderStatus != 4);
    }

    private IQueryable<OrderEntity> VerifiedOrdersQuery()
    {
        return VoucherOrdersQuery().Where(x => x.OrderStatus == 3);
    }

    private static System.Linq.Expressions.Expression<Func<OrderEntity, bool>> IsVoucherOrderExpression()
    {
        return x => x.OrderType == 3 || x.OrderType == 4;
    }

    private static bool IsVoucherOrder(OrderEntity order)
    {
        return order.OrderType is 3 or 4;
    }

    private async Task<OrderEntity?> FindVoucherOrderAsync(string code, bool tracking, CancellationToken cancellationToken)
    {
        var query = tracking ? _dbContext.Orders.Where(IsVoucherOrderExpression()) : VoucherOrdersQuery();

        if (long.TryParse(code, out var orderId) && orderId > 0)
        {
            return await query.FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
        }

        return await query.FirstOrDefaultAsync(x => x.OrderNumber == code, cancellationToken);
    }

    private static IQueryable<OrderEntity> ApplyVoucherTypeFilter(IQueryable<OrderEntity> query, string? type)
    {
        return NormalizeVoucherType(type) switch
        {
            "activity" => query.Where(x => x.OrderType == 4),
            "pick" or "picking" => query.Where(x => x.OrderType == 3),
            _ => query
        };
    }

    private static IQueryable<OrderEntity> ApplyVoucherStatusFilter(IQueryable<OrderEntity> query, string? status)
    {
        return (status ?? "unused").Trim().ToLowerInvariant() switch
        {
            "used" => query.Where(x => x.OrderStatus == 3),
            "expired" => query.Where(x => x.OrderCreationTime.AddDays(30) < DateTime.Now && x.OrderStatus != 3),
            "all" => query,
            _ => query.Where(x => x.PaymentStatus == 1 && x.OrderStatus != 3 && x.OrderStatus != 4)
        };
    }

    private async Task<Dictionary<int, User>> LoadUserMapAsync(IEnumerable<OrderEntity> orders, CancellationToken cancellationToken)
    {
        var userIds = orders.Select(x => x.UserId).Distinct().ToList();
        if (userIds.Count == 0)
        {
            return new Dictionary<int, User>();
        }

        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserId))
            .ToDictionaryAsync(x => x.UserId, cancellationToken);
    }

    private static object BuildVoucherListItem(OrderEntity order, IReadOnlyDictionary<int, User> userMap)
    {
        userMap.TryGetValue(order.UserId, out var user);
        var voucherType = MapVoucherType(order.OrderType);
        var status = MapVoucherStatus(order);

        return new
        {
            voucherId = order.OrderId.ToString(),
            voucherType,
            title = MapVoucherTitle(order),
            userName = ResolveUserName(user, order),
            userPhone = MaskPhone(user?.PhoneNumber ?? order.ContactNumber),
            orderId = order.OrderId.ToString(),
            status,
            expireTime = GetExpireTime(order).ToString("yyyy-MM-dd"),
            createTime = order.OrderCreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
            voucher_id = order.OrderId.ToString(),
            voucher_type = voucherType,
            user_name = ResolveUserName(user, order),
            user_phone = MaskPhone(user?.PhoneNumber ?? order.ContactNumber),
            order_id = order.OrderId.ToString(),
            expire_time = GetExpireTime(order).ToString("yyyy-MM-dd"),
            create_time = order.OrderCreationTime.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }

    private static object BuildHistoryItem(OrderEntity order, IReadOnlyDictionary<int, User> userMap, User staff)
    {
        userMap.TryGetValue(order.UserId, out var user);
        var voucherType = MapVoucherType(order.OrderType);
        var verifyTime = order.PaymentTime.ToString("yyyy-MM-dd HH:mm:ss");

        return new
        {
            id = order.OrderId.ToString(),
            verifyId = order.OrderId.ToString(),
            voucherType,
            title = MapVoucherTitle(order),
            userName = ResolveUserName(user, order),
            verifyTime,
            verifyStaff = ResolveUserName(staff, null),
            verify_id = order.OrderId.ToString(),
            voucher_type = voucherType,
            user_name = ResolveUserName(user, order),
            verify_time = verifyTime,
            verify_staff = ResolveUserName(staff, null)
        };
    }

    private static string NormalizeVoucherCode(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var value = Uri.UnescapeDataString(raw.Trim());
        if (value.Contains("verifyCode=", StringComparison.OrdinalIgnoreCase))
        {
            var query = value.Split('?', 2).Last();
            foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = pair.Split('=', 2);
                if (parts.Length == 2 && parts[0].Equals("verifyCode", StringComparison.OrdinalIgnoreCase))
                {
                    value = Uri.UnescapeDataString(parts[1]);
                    break;
                }
            }
        }

        foreach (var prefix in new[] { "ACT-", "PICK-" })
        {
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var withoutPrefix = value[prefix.Length..];
                var lastDash = withoutPrefix.LastIndexOf('-');
                return lastDash > 0 ? withoutPrefix[..lastDash] : withoutPrefix;
            }
        }

        return value;
    }

    private static string NormalizeVoucherType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return string.Empty;
        }

        return type.Trim().ToLowerInvariant() switch
        {
            "activity" => "activity",
            "picking" => "picking",
            "pick" => "pick",
            "acre" => "picking",
            _ => type.Trim().ToLowerInvariant()
        };
    }

    private static string MapVoucherType(int orderType)
    {
        return orderType == 4 ? "activity" : "picking";
    }

    private static string MapVoucherTitle(OrderEntity order)
    {
        return order.OrderType == 4 ? "活动体验券" : "采摘认购券";
    }

    private static string MapVoucherStatus(OrderEntity order)
    {
        if (order.OrderStatus == 3)
        {
            return "used";
        }

        if (GetExpireTime(order) < DateTime.Now)
        {
            return "expired";
        }

        return "unused";
    }

    private static DateTime GetExpireTime(OrderEntity order)
    {
        return order.OrderCreationTime.AddDays(30);
    }

    private static bool IsStaffRole(string? roleName)
    {
        return !string.IsNullOrWhiteSpace(roleName) &&
               (roleName.Trim().Equals("staff", StringComparison.OrdinalIgnoreCase) ||
                roleName.Contains("员工", StringComparison.OrdinalIgnoreCase));
    }

    private static string ResolveUserName(User? user, OrderEntity? order)
    {
        if (!string.IsNullOrWhiteSpace(user?.RealName))
        {
            return user.RealName;
        }

        if (!string.IsNullOrWhiteSpace(user?.WxName))
        {
            return user.WxName;
        }

        if (!string.IsNullOrWhiteSpace(order?.ContactPerson))
        {
            return order.ContactPerson;
        }

        return "未知用户";
    }

    private static string MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 7)
        {
            return phone ?? string.Empty;
        }

        return $"{phone[..3]}****{phone[^4..]}";
    }

    public sealed class VerifyVoucherRequest
    {
        public string Code { get; set; } = string.Empty;
    }
}
