using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using WebAPI.Common;
using WebAPI.Data;
using WebAPI.Dtos;
using WebAPI.Entities;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly JwtHelper _jwtHelper;

    public AuthController(
        IAuthService authService,
        AppDbContext dbContext,
        IConfiguration configuration,
        JwtHelper jwtHelper,
        IHttpClientFactory httpClientFactory)
    {
        _authService = authService;
        _dbContext = dbContext;
        _configuration = configuration;
        _jwtHelper = jwtHelper;

        _config = configuration;
        _httpClient = httpClientFactory.CreateClient();
    }

    /// <summary>
    /// 微信登录
    /// 前端只传 code
    /// </summary>
    [HttpPost("wxlogin")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResult>> WxLogin(
        [FromBody] WechatLoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
            {
                return Ok(ApiResult.Fail("code is required"));
            }

            var appId = _configuration["WeChat:AppId"];
            var appSecret = _configuration["WeChat:AppSecret"];

            Console.WriteLine("========== WxLogin Start ==========");
            Console.WriteLine($"[WxLogin] Code: {request.Code}");
            Console.WriteLine($"[WxLogin] AppId Exists: {!string.IsNullOrWhiteSpace(appId)}");
            Console.WriteLine($"[WxLogin] AppSecret Exists: {!string.IsNullOrWhiteSpace(appSecret)}");

            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appSecret))
            {
                Console.WriteLine("[WxLogin] WeChat config missing");
                return Ok(ApiResult.Fail("wechat config missing"));
            }

            var wxSession = await GetWechatSessionAsync(appId, appSecret, request.Code, cancellationToken);

            Console.WriteLine($"[WxLogin] wxSession null: {wxSession is null}");
            Console.WriteLine($"[WxLogin] OpenId: {wxSession?.OpenId}");
            Console.WriteLine($"[WxLogin] ErrCode: {wxSession?.ErrCode}");
            Console.WriteLine($"[WxLogin] ErrMsg: {wxSession?.ErrMsg}");

            if (wxSession is null)
            {
                return Ok(ApiResult.Fail("wechat login failed"));
            }

            if (wxSession.ErrCode.HasValue && wxSession.ErrCode.Value != 0)
            {
                return Ok(ApiResult.Fail(wxSession.ErrMsg ?? "wechat login failed", wxSession.ErrCode.Value));
            }

            if (string.IsNullOrWhiteSpace(wxSession.OpenId))
            {
                return Ok(ApiResult.Fail("openid is empty"));
            }

            var openId = wxSession.OpenId.Trim();

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.WxOpenId == openId, cancellationToken);

            var isNewUser = false;

            Console.WriteLine($"[WxLogin] user exists: {user is not null}");

            if (user is null)
            {
                isNewUser = true;

                user = await CreateWechatUserAsync(openId, cancellationToken);

                Console.WriteLine($"[WxLogin] create new user, UserNo(user_guid): {user.UserNo}");

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync(cancellationToken);

                Console.WriteLine($"[WxLogin] new user saved, UserId: {user.UserId}");
            }

            var token = _jwtHelper.GenerateToken(user);

            Console.WriteLine($"[WxLogin] token generated, UserId: {user.UserId}");
            Console.WriteLine("========== WxLogin End ==========");

            return Ok(ApiResult.Success(new
            {
                token,
                isNewUser,
                user_id = user.UserId,
                user_guid = user.UserNo,
                register_time = user.RegisterTime,
                openid = user.WxOpenId,
                phone_number = user.PhoneNumber
            }));
        }
        catch (Exception ex)
        {
            Console.WriteLine("========== WxLogin ERROR ==========");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("===================================");

            return Ok(ApiResult.Fail("服务器异常，请稍后重试"));
        }
    }

    /// <summary>
    /// 检查登录状态
    /// </summary>
    [HttpGet("check")]
    [Authorize]
    public async Task<ActionResult<ApiResult>> Check(CancellationToken cancellationToken)
    {
        try
        {
            var userId = TryGetCurrentUserId();
            if (userId is null)
            {
                return Ok(ApiResult.Fail("login state invalid", 401));
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.UserId == userId.Value, cancellationToken);

            if (user is null)
            {
                return Ok(ApiResult.Fail("user not found", 404));
            }

            return Ok(ApiResult.Success(new
            {
                isLogin = true,
                isLoggedIn = true,
                user_id = user.UserId,
                user_guid = user.UserNo,
                register_time = user.RegisterTime,
                openid = user.WxOpenId,
                phone_number = user.PhoneNumber
            }));
        }
        catch (Exception ex)
        {
            Console.WriteLine("========== Check ERROR ==========");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("=================================");

            return Ok(ApiResult.Fail("服务器异常，请稍后重试"));
        }
    }

    /// <summary>
    /// 获取并保存微信手机号
    /// 前端传入 getPhoneNumber 返回的 code
    /// 需要登录后调用
    /// </summary>
    [HttpPost("phone")]
    [Authorize]
    public async Task<ActionResult<ApiResult>> GetPhoneNumber(
        [FromBody] AuthPhoneCodeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
            {
                return Ok(ApiResult.Fail("code不能为空"));
            }

            var userId = TryGetCurrentUserId();
            if (userId is null)
            {
                return Ok(ApiResult.Fail("登录状态无效", 401));
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.UserId == userId.Value, cancellationToken);

            if (user is null)
            {
                return Ok(ApiResult.Fail("用户不存在", 404));
            }

            string appId = _config["WeChat:AppId"] ?? string.Empty;
            string appSecret = _config["WeChat:AppSecret"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appSecret))
            {
                return Ok(ApiResult.Fail("微信配置缺失"));
            }

            string tokenUrl =
                $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={Uri.EscapeDataString(appId)}&secret={Uri.EscapeDataString(appSecret)}";

            using var tokenResp = await _httpClient.GetAsync(tokenUrl, cancellationToken);
            var tokenJson = await tokenResp.Content.ReadAsStringAsync(cancellationToken);

            Console.WriteLine("========== GetPhone token response ==========");
            Console.WriteLine(tokenJson);
            Console.WriteLine("============================================");

            tokenResp.EnsureSuccessStatusCode();

            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);

            if (tokenData.TryGetProperty("errcode", out var errCode) && errCode.GetInt32() != 0)
            {
                string errMsg = tokenData.TryGetProperty("errmsg", out var errmsgValue)
                    ? errmsgValue.GetString() ?? "获取token失败"
                    : "获取token失败";

                return Ok(ApiResult.Fail(errMsg));
            }

            string accessToken = tokenData.GetProperty("access_token").GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Ok(ApiResult.Fail("access_token获取失败"));
            }

            string phoneUrl =
                $"https://api.weixin.qq.com/wxa/business/getuserphonenumber?access_token={Uri.EscapeDataString(accessToken)}";

            var postData = new { code = request.Code };
            using var content = new StringContent(
                JsonSerializer.Serialize(postData),
                Encoding.UTF8,
                "application/json");

            using var phoneResp = await _httpClient.PostAsync(phoneUrl, content, cancellationToken);
            var phoneJson = await phoneResp.Content.ReadAsStringAsync(cancellationToken);

            Console.WriteLine("========== GetPhone phone response ==========");
            Console.WriteLine(phoneJson);
            Console.WriteLine("============================================");

            phoneResp.EnsureSuccessStatusCode();

            var phoneData = JsonSerializer.Deserialize<JsonElement>(phoneJson);

            int phoneErrCode = phoneData.TryGetProperty("errcode", out var phoneErrCodeValue)
                ? phoneErrCodeValue.GetInt32()
                : -1;

            if (phoneErrCode != 0)
            {
                string phoneErrMsg = phoneData.TryGetProperty("errmsg", out var phoneErrMsgValue)
                    ? phoneErrMsgValue.GetString() ?? "获取手机号失败"
                    : "获取手机号失败";

                return Ok(ApiResult.Fail(phoneErrMsg, phoneErrCode));
            }

            if (!phoneData.TryGetProperty("phone_info", out var phoneInfo))
            {
                return Ok(ApiResult.Fail("未返回手机号信息"));
            }

            var phoneNumber = phoneInfo.TryGetProperty("phoneNumber", out var phoneNumberValue)
                ? phoneNumberValue.GetString() ?? string.Empty
                : string.Empty;

            var purePhoneNumber = phoneInfo.TryGetProperty("purePhoneNumber", out var purePhoneNumberValue)
                ? purePhoneNumberValue.GetString() ?? string.Empty
                : string.Empty;

            var countryCode = phoneInfo.TryGetProperty("countryCode", out var countryCodeValue)
                ? countryCodeValue.GetString() ?? string.Empty
                : string.Empty;

            user.PhoneNumber = purePhoneNumber;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResult.Success(new
            {
                user_id = user.UserId,
                user_guid = user.UserNo,
                phoneNumber,
                purePhoneNumber,
                countryCode
            }));
        }
        catch (Exception ex)
        {
            Console.WriteLine("========== GetPhoneNumber ERROR ==========");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("=========================================");

            return Ok(ApiResult.Fail("服务器异常，请稍后重试"));
        }
    }

    private int? TryGetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("userId");

        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }

    /// <summary>
    /// 调用微信 jscode2session
    /// </summary>
    private async Task<WechatSessionResponse?> GetWechatSessionAsync(
        string appId,
        string appSecret,
        string code,
        CancellationToken cancellationToken)
    {
        var url =
            $"https://api.weixin.qq.com/sns/jscode2session?appid={Uri.EscapeDataString(appId)}&secret={Uri.EscapeDataString(appSecret)}&js_code={Uri.EscapeDataString(code)}&grant_type=authorization_code";

        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(url, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        Console.WriteLine("========== WeChat API Response ==========");
        Console.WriteLine(content);
        Console.WriteLine("=========================================");

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"wechat api error: {response.StatusCode}, content: {content}");
        }

        return JsonSerializer.Deserialize<WechatSessionResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    /// <summary>
    /// 自动创建微信用户
    /// </summary>
    private async Task<User> CreateWechatUserAsync(
        string openId,
        CancellationToken cancellationToken)
    {
        var roleId = await _dbContext.Roles
            .OrderBy(x => x.RoleId)
            .Select(x => x.RoleId)
            .FirstOrDefaultAsync(cancellationToken);

        if (roleId <= 0)
        {
            var role = new Role
            {
                RoleName = "默认角色"
            };

            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync(cancellationToken);
            roleId = role.RoleId;
        }

        return new User
        {
            UserNo = Guid.NewGuid().ToString("N"),
            PhoneNumber = string.Empty,
            RegisterTime = DateTime.Now,
            WxOpenId = openId,
            WxImage = string.Empty,
            WxName = string.Empty,
            RoleId = roleId
        };
    }

    /// <summary>
    /// 微信返回结构
    /// </summary>
    private sealed class WechatSessionResponse
    {
        [JsonPropertyName("openid")]
        public string? OpenId { get; set; }

        [JsonPropertyName("session_key")]
        public string? SessionKey { get; set; }

        [JsonPropertyName("unionid")]
        public string? UnionId { get; set; }

        [JsonPropertyName("errcode")]
        public int? ErrCode { get; set; }

        [JsonPropertyName("errmsg")]
        public string? ErrMsg { get; set; }
    }

    /// <summary>
    /// 获取手机号请求参数
    /// </summary>
    public sealed class AuthPhoneCodeRequest
    {
        public string Code { get; set; } = string.Empty;
    }
}