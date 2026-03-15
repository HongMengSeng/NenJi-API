using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Common;
using WebAPI.Dtos;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResult>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginByDeviceAsync(request, cancellationToken);
        return Ok(ApiResult.Success(response));
    }

    [HttpPost("wechat")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResult>> Wechat([FromBody] WechatLoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginByWechatAsync(request, cancellationToken);
        return Ok(ApiResult.Success(response));
    }

    [HttpPost("phone")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResult>> Phone([FromBody] PhoneLoginRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = TryGetCurrentUserId();
        var response = await _authService.LoginByPhoneAsync(currentUserId, request, cancellationToken);
        return Ok(ApiResult.Success(response));
    }

    [HttpPost("logout")]
    [Authorize]
    public ActionResult<ApiResult> Logout()
    {
        return Ok(ApiResult.Success(message: "logout success"));
    }

    [HttpGet("check")]
    [Authorize]
    public async Task<ActionResult<ApiResult>> Check(CancellationToken cancellationToken)
    {
        var userId = TryGetCurrentUserId();
        if (userId is null)
        {
            return Ok(ApiResult.Fail("登录状态无效", 401));
        }

        var user = await _authService.GetCurrentUserAsync(userId.Value, cancellationToken);
        if (user is null)
        {
            return Ok(ApiResult.Fail("用户不存在", 404));
        }

        return Ok(ApiResult.Success(new
        {
            isLogin = true,
            isLoggedIn = true,
            user,
            userInfo = user
        }));
    }

    private int? TryGetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("userId");
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
