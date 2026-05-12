using Microsoft.AspNetCore.Mvc;
using ManageAPI.Common;
using ManageAPI.Dtos;
using ManageAPI.Services;

namespace ManageAPI.Controllers;

[ApiController]
[Route("api/coupon")]
public class CouponController : ControllerBase
{
    private readonly ICouponService _couponService;
    private readonly ILogger<CouponController> _logger;

    public CouponController(ICouponService couponService, ILogger<CouponController> logger)
    {
        _couponService = couponService;
        _logger = logger;
    }

    /// <summary>
    /// 获取券品列表
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 15,
        [FromQuery] string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (records, total) = await _couponService.GetCouponListAsync(
                pageNum, pageSize, keyword, cancellationToken);

            return Ok(ApiResult.Success(new
            {
                records,
                total,
                pageNum,
                pageSize,
                pages = (total + pageSize - 1) / pageSize
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError($"获取券品列表失败: {ex.Message}");
            return Ok(ApiResult.Fail("获取失败", 500));
        }
    }

    /// <summary>
    /// 获取券品详情
    /// </summary>
    [HttpGet("detail")]
    public async Task<IActionResult> GetDetail(
        [FromQuery] long id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
                return Ok(ApiResult.Fail("券品ID不能为空", 400));

            var coupon = await _couponService.GetCouponDetailAsync(id, cancellationToken);
            if (coupon is null)
                return Ok(ApiResult.Fail("券品不存在或已被删除", 404));

            return Ok(ApiResult.Success(coupon));
        }
        catch (Exception ex)
        {
            _logger.LogError($"获取券品详情失败: {ex.Message}");
            return Ok(ApiResult.Fail("获取失败", 500));
        }
    }

    /// <summary>
    /// 创建券品
    /// </summary>
    [HttpPost("add")]
    public async Task<IActionResult> Create(
        [FromBody] CreateCouponDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return Ok(ApiResult.Fail("输入参数格式不正确", 400));

            var id = await _couponService.CreateCouponAsync(dto, cancellationToken);
            return Ok(ApiResult.Success(new { id }));
        }
        catch (Exception ex)
        {
            _logger.LogError($"创建券品失败: {ex.Message}");
            return Ok(ApiResult.Fail("创建失败", 500));
        }
    }

    /// <summary>
    /// 编辑券品
    /// </summary>
    [HttpPut("edit")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateCouponDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid || dto.Id <= 0)
                return Ok(ApiResult.Fail("输入参数格式不正确", 400));

            var success = await _couponService.UpdateCouponAsync(dto.Id, dto, cancellationToken);
            if (!success)
                return Ok(ApiResult.Fail("券品不存在或已被删除", 404));

            return Ok(ApiResult.Success("编辑成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError($"编辑券品失败: {ex.Message}");
            return Ok(ApiResult.Fail("编辑失败", 500));
        }
    }

    /// <summary>
    /// 删除券品
    /// </summary>
    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request?.Id <= 0)
                return Ok(ApiResult.Fail("券品ID不能为空", 400));

            var success = await _couponService.DeleteCouponAsync(request.Id, cancellationToken);
            if (!success)
                return Ok(ApiResult.Fail("券品不存在或已被删除", 404));

            return Ok(ApiResult.Success("删除成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError($"删除券品失败: {ex.Message}");
            return Ok(ApiResult.Fail("删除失败", 500));
        }
    }

    /// <summary>
    /// 批量删除券品
    /// </summary>
    [HttpPost("deleteBatch")]
    public async Task<IActionResult> DeleteBatch(
        [FromBody] DeleteBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request?.Ids == null || request.Ids.Length == 0)
                return Ok(ApiResult.Fail("券品ID不能为空", 400));

            var success = await _couponService.DeleteCouponBatchAsync(request.Ids, cancellationToken);
            if (!success)
                return Ok(ApiResult.Fail("没有找到可删除的券品", 404));

            return Ok(ApiResult.Success("批量删除成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError($"批量删除券品失败: {ex.Message}");
            return Ok(ApiResult.Fail("删除失败", 500));
        }
    }
}

/// <summary>
/// 删除请求
/// </summary>
public class DeleteRequest
{
    public long Id { get; set; }
}

/// <summary>
/// 批量删除请求
/// </summary>
public class DeleteBatchRequest
{
    public long[] Ids { get; set; } = [];
}
