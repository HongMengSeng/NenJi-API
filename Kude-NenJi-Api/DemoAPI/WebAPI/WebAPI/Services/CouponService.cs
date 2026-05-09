using Microsoft.EntityFrameworkCore;
using WebAPI.Common;
using WebAPI.Data;
using WebAPI.Dtos;
using WebAPI.Entities;

namespace WebAPI.Services;

/// <summary>
/// 券品管理服务 - 基于活动表实现
/// </summary>
public class CouponService : ICouponService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<CouponService> _logger;

    public CouponService(AppDbContext dbContext, ILogger<CouponService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 获取券品列表
    /// </summary>
    public async Task<(List<CouponListItemDto> Records, int Total)> GetCouponListAsync(
        int pageNum, int pageSize, string? keyword, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Activities
            .AsNoTracking()
            .Where(a => a.Status != 0);  // 排除已删除的

        // 关键词搜索
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(a =>
                a.Title.Contains(kw) ||
                a.DateText.Contains(kw));
        }

        var total = await query.CountAsync(cancellationToken);

        var activities = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var records = new List<CouponListItemDto>();

        foreach (var activity in activities)
        {
            // 解析活动字段到券品字段
            var couponItem = MapActivityToCouponListItem(activity);
            records.Add(couponItem);
        }

        return (records, total);
    }

    /// <summary>
    /// 获取券品详情
    /// </summary>
    public async Task<CouponDetailDto?> GetCouponDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        var activity = await _dbContext.Activities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ActivityId == id && a.Status != 0, cancellationToken);

        if (activity is null)
            return null;

        // 加载素材
        var materials = await _dbContext.ActivityMaterials
            .AsNoTracking()
            .Where(m => m.ActivityId == id)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);

        return MapActivityToCouponDetail(activity, materials);
    }

    /// <summary>
    /// 新增券品
    /// </summary>
    public async Task<long> CreateCouponAsync(CreateCouponDto dto, CancellationToken cancellationToken = default)
    {
        var activity = new ActivityEntity
        {
            Title = dto.Name,
            PriceText = (int)dto.Price,
            DateText = $"{dto.ValidityPeriod}{dto.ValidityUnit}",
            ImageUrl = dto.Image ?? string.Empty,
            Participants = (int)dto.Stock,  // 库存当参加人数
            RemainingSlots = (int)dto.Stock,
            Status = 1,  // 默认上架
            SortOrder = 999,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Activities.Add(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"券品新增成功 - ActivityId: {activity.ActivityId}, Title: {activity.Title}");

        // 保存轮播图素材
        if (dto.CarouselMedia?.Count > 0)
        {
            var materials = dto.CarouselMedia
                .Select((m, idx) => new ActivityMaterial
                {
                    ActivityId = activity.ActivityId,
                    MaterialType = "0",  // 0=图片
                    MaterialUrl = m.Url,
                    SortOrder = idx,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            _dbContext.ActivityMaterials.AddRange(materials);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return activity.ActivityId;
    }

    /// <summary>
    /// 编辑券品
    /// </summary>
    public async Task<bool> UpdateCouponAsync(long id, UpdateCouponDto dto, CancellationToken cancellationToken = default)
    {
        var activity = await _dbContext.Activities
            .FirstOrDefaultAsync(a => a.ActivityId == id && a.Status != 0, cancellationToken);

        if (activity is null)
            return false;

        activity.Title = dto.Name;
        activity.PriceText = (int)dto.Price;
        activity.DateText = $"{dto.ValidityPeriod}{dto.ValidityUnit}";
        activity.ImageUrl = dto.Image ?? string.Empty;
        activity.Participants = (int)dto.Stock;
        activity.RemainingSlots = (int)dto.Stock;

        // 删除旧素材
        var oldMaterials = await _dbContext.ActivityMaterials
            .Where(m => m.ActivityId == id)
            .ToListAsync(cancellationToken);

        _dbContext.ActivityMaterials.RemoveRange(oldMaterials);

        // 添加新素材
        if (dto.CarouselMedia?.Count > 0)
        {
            var materials = dto.CarouselMedia
                .Select((m, idx) => new ActivityMaterial
                {
                    ActivityId = activity.ActivityId,
                    MaterialType = "0",
                    MaterialUrl = m.Url,
                    SortOrder = idx,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            _dbContext.ActivityMaterials.AddRange(materials);
        }

        _dbContext.Activities.Update(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"券品编辑成功 - ActivityId: {id}");

        return true;
    }

    /// <summary>
    /// 删除券品
    /// </summary>
    public async Task<bool> DeleteCouponAsync(long id, CancellationToken cancellationToken = default)
    {
        var activity = await _dbContext.Activities
            .FirstOrDefaultAsync(a => a.ActivityId == id && a.Status != 0, cancellationToken);

        if (activity is null)
            return false;

        // 标记为删除（软删除）
        activity.Status = 0;
        _dbContext.Activities.Update(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"券品删除成功 - ActivityId: {id}");

        return true;
    }

    /// <summary>
    /// 批量删除券品
    /// </summary>
    public async Task<bool> DeleteCouponBatchAsync(long[] ids, CancellationToken cancellationToken = default)
    {
        var activities = await _dbContext.Activities
            .Where(a => ids.Contains(a.ActivityId) && a.Status != 0)
            .ToListAsync(cancellationToken);

        if (activities.Count == 0)
            return false;

        foreach (var activity in activities)
        {
            activity.Status = 0;
        }

        _dbContext.Activities.UpdateRange(activities);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"批量删除券品成功 - 数量: {activities.Count}");

        return true;
    }

    /// <summary>
    /// 映射：活动 -> 券品列表项
    /// </summary>
    private static CouponListItemDto MapActivityToCouponListItem(ActivityEntity activity)
    {
        var (validityPeriod, validityUnit) = ParseDateText(activity.DateText);

        return new CouponListItemDto
        {
            Id = activity.ActivityId,
            Name = activity.Title,
            Type = InferCouponType(activity.Title),  // 从标题推断类型
            Price = activity.PriceText,
            Stock = activity.Participants,
            LimitPerOrder = 4,  // 默认值
            ValidityPeriod = validityPeriod,
            ValidityUnit = validityUnit,
            Validity = activity.DateText,
            RefundRule = "需人工审核退款",  // 默认值
            UsageRules = "详见券品详情",  // 默认值
            Image = activity.ImageUrl,
            CarouselMedia = [],  // 列表不返回
            SoldCount = activity.Participants - activity.RemainingSlots,
            VerifiedCount = 0,
            CreateTime = activity.CreatedAt.ToString("yyyy-MM-dd HH:mm")
        };
    }

    /// <summary>
    /// 映射：活动 -> 券品详情
    /// </summary>
    private static CouponDetailDto MapActivityToCouponDetail(
        ActivityEntity activity,
        List<ActivityMaterial> materials)
    {
        var (validityPeriod, validityUnit) = ParseDateText(activity.DateText);

        var carouselMedia = materials
            .Select(m => new CarouselMediaDto
            {
                Type = m.MaterialType == "2" ? "video" : "image",
                Url = m.MaterialUrl,
                Thumb = null
            })
            .ToList();

        return new CouponDetailDto
        {
            Id = activity.ActivityId,
            Name = activity.Title,
            Type = InferCouponType(activity.Title),
            Price = activity.PriceText,
            Stock = activity.Participants,
            LimitPerOrder = 4,
            ValidityPeriod = validityPeriod,
            ValidityUnit = validityUnit,
            Validity = activity.DateText,
            RefundRule = "需人工审核退款",
            UsageRules = "详见券品详情",
            Image = activity.ImageUrl,
            ImageName = Path.GetFileName(activity.ImageUrl),
            CarouselMedia = carouselMedia,
            SoldCount = activity.Participants - activity.RemainingSlots,
            VerifiedCount = 0,
            CreateTime = activity.CreatedAt.ToString("yyyy-MM-dd HH:mm")
        };
    }

    /// <summary>
    /// 从DateText解析有效期
    /// 格式：如 "30天" 或 "3个月"
    /// </summary>
    private static (int period, string unit) ParseDateText(string dateText)
    {
        if (string.IsNullOrEmpty(dateText))
            return (30, "天");

        // 提取数字
        var numberStr = new string(dateText.Where(char.IsDigit).ToArray());
        if (!int.TryParse(numberStr, out var period))
            period = 30;

        // 推断单位
        var unit = "天";
        if (dateText.Contains("月"))
            unit = "月";
        else if (dateText.Contains("年"))
            unit = "年";

        return (period, unit);
    }

    /// <summary>
    /// 从标题推断券品类型
    /// </summary>
    private static string InferCouponType(string title)
    {
        if (title.Contains("采摘") || title.Contains("采"))
            return "采摘券";
        if (title.Contains("研学") || title.Contains("学"))
            return "研学活动券";
        return "活动券";
    }
}