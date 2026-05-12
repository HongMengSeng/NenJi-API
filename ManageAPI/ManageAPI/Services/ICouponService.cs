using ManageAPI.Dtos;

namespace ManageAPI.Services;

/// <summary>
/// 券品管理服务接口
/// </summary>
public interface ICouponService
{
    /// <summary>
    /// 获取券品列表
    /// </summary>
    Task<(List<CouponListItemDto> Records, int Total)> GetCouponListAsync(
        int pageNum, int pageSize, string? keyword, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取券品详情
    /// </summary>
    Task<CouponDetailDto?> GetCouponDetailAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建券品
    /// </summary>
    Task<long> CreateCouponAsync(CreateCouponDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 编辑券品
    /// </summary>
    Task<bool> UpdateCouponAsync(long id, UpdateCouponDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除券品
    /// </summary>
    Task<bool> DeleteCouponAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量删除券品
    /// </summary>
    Task<bool> DeleteCouponBatchAsync(long[] ids, CancellationToken cancellationToken = default);
}
