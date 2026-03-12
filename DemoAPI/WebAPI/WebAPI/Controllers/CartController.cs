using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Common;
using WebAPI.Dtos;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly IAppService _appService;

    public CartController(IAppService appService)
    {
        _appService = appService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResult>> List(CancellationToken cancellationToken)
    {
        var data = await _appService.GetCartListAsync(GetCurrentUserId(), cancellationToken);
        return Ok(ApiResult.Success(data));
    }

    [HttpPost("add")]
    public async Task<ActionResult<ApiResult>> Add([FromBody] CartAddRequest request, CancellationToken cancellationToken)
    {
        await _appService.AddCartItemAsync(GetCurrentUserId(), request, cancellationToken);
        return Ok(ApiResult.Success(message: "success"));
    }

    [HttpPut("update")]
    public async Task<ActionResult<ApiResult>> Update([FromBody] CartUpdateRequest request, CancellationToken cancellationToken)
    {
        await _appService.UpdateCartItemAsync(GetCurrentUserId(), request, cancellationToken);
        return Ok(ApiResult.Success(message: "success"));
    }

    [HttpDelete("delete")]
    public async Task<ActionResult<ApiResult>> Delete([FromBody] CartDeleteRequest request, CancellationToken cancellationToken)
    {
        await _appService.DeleteCartItemAsync(GetCurrentUserId(), request.CartId, cancellationToken);
        return Ok(ApiResult.Success(message: "success"));
    }

    [HttpDelete("clear")]
    public async Task<ActionResult<ApiResult>> Clear(CancellationToken cancellationToken)
    {
        await _appService.ClearCartAsync(GetCurrentUserId(), cancellationToken);
        return Ok(ApiResult.Success(message: "success"));
    }

    private int GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("userId");
        return int.TryParse(userIdValue, out var userId) ? userId : throw new InvalidOperationException("未授权，请重新登录");
    }
}
