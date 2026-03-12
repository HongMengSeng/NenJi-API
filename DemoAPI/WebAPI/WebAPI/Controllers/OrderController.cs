using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Common;
using WebAPI.Dtos;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/order")]
public class OrderController : ControllerBase
{
    private readonly IAppService _appService;
    private readonly IContentService _contentService;

    public OrderController(IAppService appService, IContentService contentService)
    {
        _appService = appService;
        _contentService = contentService;
    }

    [AllowAnonymous]
    [HttpGet("getOrderData")]
    public async Task<ActionResult<ApiResult>> GetOrderData(CancellationToken cancellationToken)
    {
        var data = await _contentService.GetOrderMenuDataAsync(cancellationToken);
        return Ok(ApiResult.Success(data));
    }

    [AllowAnonymous]
    [HttpGet("goods/detail")]
    public async Task<ActionResult<ApiResult>> GoodsDetail([FromQuery] int goodsId, CancellationToken cancellationToken)
    {
        var data = await _contentService.GetOrderGoodsDetailAsync(goodsId, cancellationToken);
        return data is null
            ? Ok(ApiResult.Fail("商品不存在", 404))
            : Ok(ApiResult.Success(data));
    }

    [HttpGet("cart")]
    public async Task<ActionResult<ApiResult>> Cart(CancellationToken cancellationToken)
    {
        var data = await _contentService.GetOrderCartAsync(GetCurrentUserId(), cancellationToken);
        return Ok(ApiResult.Success(data));
    }

    [HttpPost("cart/add")]
    public async Task<ActionResult<ApiResult>> AddCart([FromBody] OrderCartAddRequest request, CancellationToken cancellationToken)
    {
        var data = await _contentService.AddToOrderCartAsync(GetCurrentUserId(), request, cancellationToken);
        return Ok(ApiResult.Success(data));
    }

    [HttpPut("cart/update")]
    public async Task<ActionResult<ApiResult>> UpdateCart([FromBody] OrderCartUpdateRequest request, CancellationToken cancellationToken)
    {
        var data = await _contentService.UpdateOrderCartAsync(GetCurrentUserId(), request, cancellationToken);
        return Ok(ApiResult.Success(data));
    }

    [HttpDelete("cart/clear")]
    public async Task<ActionResult<ApiResult>> ClearCart(CancellationToken cancellationToken)
    {
        var data = await _contentService.ClearOrderCartAsync(GetCurrentUserId(), cancellationToken);
        return Ok(ApiResult.Success(data));
    }

    [HttpPost("submit")]
    public async Task<ActionResult<ApiResult>> Submit([FromBody] SubmitMealOrderRequest request, CancellationToken cancellationToken)
    {
        var data = await _contentService.SubmitMealOrderAsync(GetCurrentUserId(), request, cancellationToken);
        return Ok(ApiResult.Success(data));
    }

    [HttpPost("create")]
    public async Task<ActionResult<ApiResult>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var orderId = await _appService.CreateOrderAsync(GetCurrentUserId(), request, cancellationToken);
        return Ok(ApiResult.Success(new { orderId }));
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResult>> List([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var data = await _appService.GetOrderListAsync(GetCurrentUserId(), status, page <= 0 ? 1 : page, pageSize <= 0 ? 10 : pageSize, cancellationToken);
        return Ok(ApiResult.Success(data));
    }

    [HttpGet("detail")]
    public async Task<ActionResult<ApiResult>> Detail([FromQuery] long orderId, CancellationToken cancellationToken)
    {
        var data = await _appService.GetOrderDetailAsync(GetCurrentUserId(), orderId, cancellationToken);
        return data is null
            ? Ok(ApiResult.Fail("订单不存在", 1004))
            : Ok(ApiResult.Success(data));
    }

    [HttpPut("cancel")]
    public async Task<ActionResult<ApiResult>> Cancel([FromBody] CancelOrderRequest request, CancellationToken cancellationToken)
    {
        var success = await _appService.CancelOrderAsync(GetCurrentUserId(), request.OrderId, cancellationToken);
        return success
            ? Ok(ApiResult.Success(message: "success"))
            : Ok(ApiResult.Fail("订单不存在", 1004));
    }

    private int GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("userId");
        return int.TryParse(userIdValue, out var userId) ? userId : throw new InvalidOperationException("未授权，请重新登录");
    }
}
