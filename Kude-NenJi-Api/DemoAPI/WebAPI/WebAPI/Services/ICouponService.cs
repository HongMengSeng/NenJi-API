using WebAPI.Dtos;

namespace WebAPI.Services;

/// <summary>
/// 券品管理服务接口
/// </summary>
public interface ICouponService
{
    /// <summary>
    /// 获取券品列表
    /// </summary>
    //Task<(List<CouponListItemDto> Records, int Total)> GetCouponListAsync(
    //    int pageNum,
    //    int pageSize,
    //    string? keyword,
    //    CancellationToken cancellationToken = default);

    ///// <summary>
    ///// 获取券品详情
    ///// </summary>
    //Task<CouponDetailDto?> GetCouponDetailAsync(string couponCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增券品
    /// </summary>
    Task<string> CreateCouponAsync(CreateCouponDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 编辑券品
    /// </summary>
    Task<bool> UpdateCouponAsync(UpdateCouponDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除券品
    /// </summary>
    Task<bool> DeleteCouponAsync(string couponCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量删除券品
    /// </summary>
    Task<bool> DeleteCouponBatchAsync(string[] couponCodes, CancellationToken cancellationToken = default);
}