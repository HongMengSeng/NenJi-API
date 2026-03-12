using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Common;
using WebAPI.Dtos;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IAppService _appService;

    public UserController(IAppService appService)
    {
        _appService = appService;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResult>> Profile(CancellationToken cancellationToken)
    {
        var data = await _appService.GetUserProfileAsync(GetCurrentUserId(), cancellationToken);
        return data is null
            ? Ok(ApiResult.Fail("用户不存在", 404))
            : Ok(ApiResult.Success(data));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ApiResult>> UpdateProfile([FromBody] UpdateUserProfileRequest request, CancellationToken cancellationToken)
    {
        var success = await _appService.UpdateUserProfileAsync(GetCurrentUserId(), request, cancellationToken);
        return success
            ? Ok(ApiResult.Success(message: "success"))
            : Ok(ApiResult.Fail("用户不存在", 404));
    }

    [HttpGet("address")]
    public async Task<ActionResult<ApiResult>> Address(CancellationToken cancellationToken)
    {
        var data = await _appService.GetAddressesAsync(GetCurrentUserId(), cancellationToken);
        return Ok(ApiResult.Success(data));
    }

    [HttpPost("address")]
    public async Task<ActionResult<ApiResult>> CreateAddress([FromBody] SaveAddressRequest request, CancellationToken cancellationToken)
    {
        var id = await _appService.CreateAddressAsync(GetCurrentUserId(), request, cancellationToken);
        return Ok(ApiResult.Success(new { id }));
    }

    [HttpPut("address")]
    public async Task<ActionResult<ApiResult>> UpdateAddress([FromBody] SaveAddressRequest request, CancellationToken cancellationToken)
    {
        var success = await _appService.UpdateAddressAsync(GetCurrentUserId(), request, cancellationToken);
        return success
            ? Ok(ApiResult.Success(message: "success"))
            : Ok(ApiResult.Fail("地址不存在", 404));
    }

    [HttpDelete("address")]
    public async Task<ActionResult<ApiResult>> DeleteAddress([FromBody] DeleteAddressRequest request, CancellationToken cancellationToken)
    {
        var success = await _appService.DeleteAddressAsync(GetCurrentUserId(), request.Id, cancellationToken);
        return success
            ? Ok(ApiResult.Success(message: "success"))
            : Ok(ApiResult.Fail("地址不存在", 404));
    }

    private int GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("userId");
        return int.TryParse(userIdValue, out var userId) ? userId : throw new InvalidOperationException("未授权，请重新登录");
    }
}
