using ManageAPI.Dtos;

namespace ManageAPI.Services;

/// <summary>
/// 活动/券品管理服务接口
/// </summary>
public interface IActivityService
{
    Task<(List<CouponListItemDto> Records, int Total)> GetActivityListAsync(
        int pageNum, int pageSize, string? keyword, CancellationToken cancellationToken = default);

    Task<CouponDetailDto?> GetActivityDetailAsync(long id, CancellationToken cancellationToken = default);

    Task<long> CreateActivityAsync(CreateCouponDto dto, CancellationToken cancellationToken = default);

    Task<bool> UpdateActivityAsync(long id, UpdateCouponDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteActivityAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> DeleteActivityBatchAsync(long[] ids, CancellationToken cancellationToken = default);
}
