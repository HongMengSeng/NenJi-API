using Microsoft.AspNetCore.Mvc;

using ManageAPI.Common;
using ManageAPI.Dtos;
using ManageAPI.Services;

namespace ManageAPI.Controllers;

[ApiController]
[Route("api/activity-order")]
public class ActivityOrderController : ControllerBase
{
    private readonly IActivityOrderService _orderService;

    public ActivityOrderController(IActivityOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 15,
        [FromQuery] string? keyword = null,
        [FromQuery] int? statusId = null,
        CancellationToken cancellationToken = default)
    {
        var (records, total) = await _orderService.GetOrderListAsync(pageNum, pageSize, keyword, statusId, cancellationToken);

        return Ok(ApiResult.Success(new
        {
            records,
            total,
            pageNum,
            pageSize,
            pages = (total + pageSize - 1) / pageSize
        }));
    }

    [HttpGet("detail")]
    public async Task<IActionResult> GetDetail(
        [FromQuery] long orderId,
        CancellationToken cancellationToken = default)
    {
        if (orderId <= 0)
            return Ok(ApiResult.Fail("参数不正确", 400));

        var order = await _orderService.GetOrderDetailAsync(orderId, cancellationToken);

        if (order is null)
            return Ok(ApiResult.Fail("订单不存在或已被删除", 404));

        return Ok(ApiResult.Success(order));
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(
        [FromBody] VerifyActivityOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request?.ActivityOrderDetailsId <= 0)
            return Ok(ApiResult.Fail("参数不能为空", 400));

        try
        {
            var success = await _orderService.VerifyOrderDetailAsync(request.ActivityOrderDetailsId, cancellationToken);

            if (!success)
                return Ok(ApiResult.Fail("核销明细不存在或已被删除", 404));

            return Ok(ApiResult.Success("核销成功"));
        }
        catch (InvalidOperationException ex)
        {
            return Ok(ApiResult.Fail(ex.Message, 400));
        }
    }
}
