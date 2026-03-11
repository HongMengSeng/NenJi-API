using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.Entities;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 认证控制器，提供一键登录、微信登录、手机号登录等接口，接口路径与《API需求文档》保持一致。
        /// </summary>
        public AuthController(ApplicationDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        /// <summary>
        /// 一键登录：/api/auth/login
        /// 根据 deviceId 作为设备标识创建或查找游客用户，并返回 token 和基础用户信息。
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<object>>> Login([FromBody] LoginRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.DeviceId))
            {
                return ApiResponse<object>.Fail("deviceId 必填", 400);
            }

            // 使用 deviceId 作为 user_no，便于同一设备多次登录找到同一条用户记录
            var userNo = req.DeviceId.Trim();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserNo == userNo);

            if (user == null)
            {
                // 简单选择一个现有角色作为默认角色
                var defaultRoleId = await _db.Roles
                    .OrderBy(r => r.RoleId)
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();
                if (defaultRoleId == 0)
                {
                    // 如果没有角色数据，仍然插入，RoleId=1；若因外键失败会抛异常，交由全局处理
                    defaultRoleId = 1;
                }

                user = new User
                {
                    UserNo = userNo,
                    PhoneNumber = null,
                    RegisterTime = DateTime.UtcNow,
                    WxOpenId = null,
                    WxImage = null,
                    WxNickname = "游客",
                    RoleId = defaultRoleId
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }

            var token = GenerateSimpleToken(user);

            var data = new
            {
                token,
                userInfo = new
                {
                    id = user.UserId,
                    nickname = user.WxNickname ?? user.UserNo,
                    avatar = user.WxImage ?? string.Empty
                }
            };

            return ApiResponse<object>.Ok(data);
        }

        /// <summary>
        /// 微信登录：/api/auth/wechat
        /// 使用 wx.login 返回的 code 调用微信 jscode2session 接口换取 openid，
        /// 并将 openid 与用户表中的 wx_openid 关联，返回 token 和微信昵称/头像（如有）。
        /// </summary>
        [HttpPost("wechat")]
        public async Task<ActionResult<ApiResponse<object>>> Wechat([FromBody] WechatLoginRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Code))
            {
                return ApiResponse<object>.Fail("code 必填", 400);
            }

            var appId = _configuration["WeChat:AppId"];
            var secret = _configuration["WeChat:Secret"];
            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(secret))
            {
                return ApiResponse<object>.Fail("微信 AppId 或 Secret 未配置", 500);
            }

            var url =
                $"https://api.weixin.qq.com/sns/jscode2session?appid={Uri.EscapeDataString(appId)}&secret={Uri.EscapeDataString(secret)}&js_code={Uri.EscapeDataString(req.Code)}&grant_type=authorization_code";

            WxSessionResponse? session;
            try
            {
                using var http = new HttpClient();
                var resp = await http.GetStringAsync(url);
                session = System.Text.Json.JsonSerializer.Deserialize<WxSessionResponse>(
                    resp,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (HttpRequestException ex)
            {
                return ApiResponse<object>.Fail("调用微信登录接口失败：" + ex.Message, 502);
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail("微信登录异常：" + ex.Message, 500);
            }

            if (session == null || string.IsNullOrWhiteSpace(session.OpenId))
            {
                return ApiResponse<object>.Fail("微信返回结果无效", 502);
            }

            if (session.ErrCode.HasValue && session.ErrCode.Value != 0)
            {
                return ApiResponse<object>.Fail($"微信登录失败：{session.ErrMsg}", 1000);
            }

            // 根据 openid 查找或创建用户
            var user = await _db.Users.FirstOrDefaultAsync(u => u.WxOpenId == session.OpenId);
            if (user == null)
            {
                var defaultRoleId = await _db.Roles
                    .OrderBy(r => r.RoleId)
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();
                if (defaultRoleId == 0) defaultRoleId = 1;

                user = new User
                {
                    UserNo = "wx_" + session.OpenId,
                    PhoneNumber = null,
                    RegisterTime = DateTime.UtcNow,
                    WxOpenId = session.OpenId,
                    // 这里真实项目中应通过解密 encryptedData 获取昵称/头像
                    WxNickname = "微信用户",
                    WxImage = string.Empty,
                    RoleId = defaultRoleId
                };

                _db.Users.Add(user);
            }
            else
            {
                // 已存在用户，更新时间戳即可；如有昵称/头像可在此处更新
                user.RegisterTime ??= DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            var token = GenerateSimpleToken(user);

            var data = new
            {
                token,
                userInfo = new
                {
                    id = user.UserId,
                    nickname = user.WxNickname ?? "微信用户",
                    avatar = user.WxImage ?? string.Empty
                }
            };

            return ApiResponse<object>.Ok(data);
        }

        /// <summary>
        /// 手机号登录：/api/auth/phone
        /// 根据手机号查找或创建用户，验证码逻辑可在此处接入短信服务。
        /// </summary>
        [HttpPost("phone")]
        public async Task<ActionResult<ApiResponse<object>>> Phone([FromBody] PhoneLoginRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Phone))
            {
                return ApiResponse<object>.Fail("phone 必填", 400);
            }

            if (string.IsNullOrWhiteSpace(req.Code))
            {
                return ApiResponse<object>.Fail("验证码必填", 1001);
            }

            // 演示环境不做真实验证码校验，只要传入即认为通过
            var phone = req.Phone.Trim();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);

            if (user == null)
            {
                var defaultRoleId = await _db.Roles
                    .OrderBy(r => r.RoleId)
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();
                if (defaultRoleId == 0) defaultRoleId = 1;

                user = new User
                {
                    UserNo = "phone_" + phone,
                    PhoneNumber = phone,
                    RegisterTime = DateTime.UtcNow,
                    WxOpenId = null,
                    WxImage = null,
                    WxNickname = "手机号用户",
                    RoleId = defaultRoleId
                };

                _db.Users.Add(user);
            }

            await _db.SaveChangesAsync();

            var token = GenerateSimpleToken(user);

            var data = new
            {
                token,
                userInfo = new
                {
                    id = user.UserId,
                    nickname = user.WxNickname ?? "手机号用户",
                    avatar = user.WxImage ?? string.Empty
                }
            };

            return ApiResponse<object>.Ok(data);
        }

        /// <summary>
        /// 登出接口：目前为无状态 token，直接返回成功，前端删除本地 token 即可。
        /// </summary>
        [HttpPost("logout")]
        public ActionResult<ApiResponse<object>> Logout()
        {
            return ApiResponse<object>.Ok(null);
        }

        /// <summary>
        /// 检查登录状态：实际项目中应解析 Authorization 里的 token。
        /// 这里为了便于联调，简单返回已登录状态和一条示例用户信息。
        /// </summary>
        [HttpGet("check")]
        public ActionResult<ApiResponse<object>> Check()
        {
            var data = new
            {
                isLoggedIn = true,
                userInfo = new
                {
                    id = 0,
                    nickname = "游客",
                    avatar = string.Empty
                }
            };
            return ApiResponse<object>.Ok(data);
        }

        /// <summary>
        /// 简单的 token 生成方法，仅用于演示和前后端联调。
        /// 正式环境请替换为 JWT 等安全方案。
        /// </summary>
        private static string GenerateSimpleToken(User user)
        {
            var payload = $"{user.UserId}:{Guid.NewGuid():N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(payload);
            return Convert.ToBase64String(bytes);
        }

        private class WxSessionResponse
        {
            public string OpenId { get; set; } = string.Empty;
            public string SessionKey { get; set; } = string.Empty;
            public string? UnionId { get; set; }
            public int? ErrCode { get; set; }
            public string? ErrMsg { get; set; }
        }
    }
}