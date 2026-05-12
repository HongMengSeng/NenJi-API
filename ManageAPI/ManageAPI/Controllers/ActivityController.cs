using Microsoft.AspNetCore.Mvc;

using ManageAPI.Common;
using ManageAPI.Dtos;
using ManageAPI.Services;

namespace ManageAPI.Controllers;

/// <summary>
/// 活动/券品管理
/// </summary>
[ApiController]
[Route("api/activity")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
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
        var (records, total) = await _activityService.GetActivityListAsync(pageNum, pageSize, keyword, cancellationToken);

        return Ok(ApiResult.Success(new
        {
            records,
            total,
            pageNum,
            pageSize,
            pages = (total + pageSize - 1) / pageSize
        }));
    }

    /// <summary>
    /// 获取券品详情
    /// </summary>
    [HttpGet("detail")]
    public async Task<IActionResult> GetDetail(
        [FromQuery] long id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return Ok(ApiResult.Fail("参数不正确", 400));

        var coupon = await _activityService.GetActivityDetailAsync(id, cancellationToken);

        if (coupon is null)
            return Ok(ApiResult.Fail("券品不存在或已被删除", 404));

        return Ok(ApiResult.Success(coupon));
    }

    /// <summary>
    /// 新增券品
    /// </summary>
    [HttpPost("add")]
    public async Task<IActionResult> Create(
        [FromBody] CreateCouponDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Ok(ApiResult.Fail("券品名称不能为空", 400));

        var id = await _activityService.CreateActivityAsync(dto, cancellationToken);
        return Ok(ApiResult.Success(new { id }));
    }

    /// <summary>
    /// 编辑券品
    /// </summary>
    [HttpPut("edit")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateCouponDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.Id <= 0 || string.IsNullOrWhiteSpace(dto.Name))
            return Ok(ApiResult.Fail("参数不能为空", 400));

        var success = await _activityService.UpdateActivityAsync(dto.Id, dto, cancellationToken);

        if (!success)
            return Ok(ApiResult.Fail("券品不存在或已被删除", 404));

        return Ok(ApiResult.Success("编辑成功"));
    }

    /// <summary>
    /// 删除券品
    /// </summary>
    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteCouponRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request?.Id <= 0)
            return Ok(ApiResult.Fail("参数不能为空", 400));

        var success = await _activityService.DeleteActivityAsync(request.Id, cancellationToken);

        if (!success)
            return Ok(ApiResult.Fail("券品不存在或已被删除", 404));

        return Ok(ApiResult.Success("删除成功"));
    }

    /// <summary>
    /// 批量删除券品
    /// </summary>
    [HttpPost("deleteBatch")]
    public async Task<IActionResult> DeleteBatch(
        [FromBody] DeleteBatchCouponRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request?.Ids == null || request.Ids.Length == 0)
            return Ok(ApiResult.Fail("参数不能为空", 400));

        var success = await _activityService.DeleteActivityBatchAsync(request.Ids, cancellationToken);

        if (!success)
            return Ok(ApiResult.Fail("删除失败", 404));

        return Ok(ApiResult.Success("删除成功"));
    }
}

public class DeleteCouponRequest
{
    public long Id { get; set; }
}

public class DeleteBatchCouponRequest
{
    public long[]? Ids { get; set; }
}
