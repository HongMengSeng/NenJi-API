using Microsoft.EntityFrameworkCore;

using ManageAPI.Data;
using ManageAPI.Dtos;
using ManageAPI.Entity;

namespace ManageAPI.Services;

public class ActivityOrderService : IActivityOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ActivityOrderService> _logger;

    public ActivityOrderService(AppDbContext dbContext, ILogger<ActivityOrderService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<(List<ActivityOrderListItemDto> Records, int Total)> GetOrderListAsync(
        int pageNum, int pageSize, string? keyword, int? statusId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<ActivityOrder>()
            .AsNoTracking()
            .Include(o => o.OrderStatus)
            .Include(o => o.ActivityOrderDetails)
                .ThenInclude(d => d.Activity)
            .AsQueryable();

        if (statusId.HasValue)
        {
            query = query.Where(o => o.OrderStatusId == statusId.Value);
        }
        else
        {
            query = query.Where(o => o.OrderStatusId == 2);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(o =>
                o.OrderNo.Contains(kw) ||
                o.ActivityOrderDetails.Any(d => d.Activity.Title.Contains(kw)));
        }

        var total = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(o => o.CreateTime)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = orders.Select(o => o.UserId).Distinct().ToList();
        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.UserId))
            .ToDictionaryAsync(u => u.UserId, cancellationToken);

        var records = orders.Select(o =>
        {
            var firstDetail = o.ActivityOrderDetails.FirstOrDefault();
            return new ActivityOrderListItemDto
            {
                OrderId = o.OrderId,
                OrderNo = o.OrderNo,
                TotalAmount = o.TotalAmount,
                TotalQuantity = o.TotalQuantity,
                OrderStatusId = o.OrderStatusId,
                StatusName = o.OrderStatus?.StatusName ?? string.Empty,
                UserId = o.UserId,
                UserName = users.GetValueOrDefault(o.UserId)?.WxName ?? users.GetValueOrDefault(o.UserId)?.RealName,
                ActivityTitle = firstDetail?.Activity?.Title,
                CreateTime = o.CreateTime.ToString("yyyy-MM-dd HH:mm"),
            };
        }).ToList();

        return (records, total);
    }

    public async Task<ActivityOrderFullDetailDto?> GetOrderDetailAsync(long orderId, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Set<ActivityOrder>()
            .AsNoTracking()
            .Include(o => o.OrderStatus)
            .Include(o => o.ActivityOrderDetails)
                .ThenInclude(d => d.Activity)
            .Include(o => o.ActivityOrderDetails)
                .ThenInclude(d => d.ActivityVerificationRecords)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

        if (order is null)
            return null;

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == order.UserId, cancellationToken);

        var items = order.ActivityOrderDetails.Select(d => new ActivityOrderItemDto
        {
            ActivityOrderDetailsId = d.ActivityOrderDetailsId,
            ActivityId = d.ActivityId,
            ActivityTitle = d.Activity?.Title ?? string.Empty,
            ActivityImage = d.Activity?.ImageUrl,
            ActivityDescription = d.Activity?.Description,
            ActivityLocation = d.Activity?.Location,
            UnitPrice = d.UnitPrice,
            Quantity = d.Quantity,
            SubtotalAmount = d.SubtotalAmount,
            ActivityQrcode = d.ActivityQrcode,
            IsVerified = d.ActivityVerificationRecords.Count > 0,
            VerificationTime = d.ActivityVerificationRecords
                .OrderByDescending(v => v.VerificationTime)
                .FirstOrDefault()?.VerificationTime.ToString("yyyy-MM-dd HH:mm"),
        }).ToList();

        return new ActivityOrderFullDetailDto
        {
            OrderId = order.OrderId,
            OrderNo = order.OrderNo,
            WxPayNo = order.WxPayNo,
            TotalAmount = order.TotalAmount,
            TotalQuantity = order.TotalQuantity,
            OrderStatusId = order.OrderStatusId,
            StatusName = order.OrderStatus?.StatusName ?? string.Empty,
            UserId = order.UserId,
            UserName = user?.WxName ?? user?.RealName,
            UserPhone = user?.PhoneNumber,
            CreateTime = order.CreateTime.ToString("yyyy-MM-dd HH:mm"),
            Items = items,
        };
    }

    public async Task<bool> VerifyOrderDetailAsync(long activityOrderDetailsId, CancellationToken cancellationToken = default)
    {
        var detail = await _dbContext.ActivityOrderDetails
            .Include(d => d.ActivityOrder)
            .Include(d => d.ActivityVerificationRecords)
            .FirstOrDefaultAsync(d => d.ActivityOrderDetailsId == activityOrderDetailsId, cancellationToken);

        if (detail is null)
            return false;

        if (detail.ActivityOrder.OrderStatusId != 2)
            throw new InvalidOperationException($"当前订单状态不允许核销（状态ID: {detail.ActivityOrder.OrderStatusId}，仅待核销状态可操作）");

        if (detail.ActivityVerificationRecords.Count > 0)
            throw new InvalidOperationException("该明细已核销，不可重复核销");

        var record = new ActivityVerificationRecord
        {
            ActivityOrderDetailsId = detail.ActivityOrderDetailsId,
            VerificationTime = DateTime.Now,
        };

        _dbContext.Set<ActivityVerificationRecord>().Add(record);

        var order = detail.ActivityOrder;
        var allDetailsForOrder = await _dbContext.ActivityOrderDetails
            .Include(d => d.ActivityVerificationRecords)
            .Where(d => d.ActivityOrderId == order.OrderId)
            .ToListAsync(cancellationToken);

        var allVerified = allDetailsForOrder.All(d =>
            d.ActivityOrderDetailsId == activityOrderDetailsId
                ? true
                : d.ActivityVerificationRecords.Count > 0);

        if (allVerified)
        {
            order.OrderStatusId = 3;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("核销成功 - DetailId: {DetailId}, OrderId: {OrderId}, AllVerified: {AllVerified}",
            activityOrderDetailsId, order.OrderId, allVerified);

        return true;
    }
}
