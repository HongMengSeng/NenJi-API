using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// 个人中心相关接口，从 user / shipping_address / order 表中读取数据。
        /// </summary>
        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Used by: demo/pages/profile/profile.js (个人中心)
        #region GetCurrent - demo/pages/profile/profile.js个人中心
        [HttpGet]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrent()
        {
            // 暂时没有登录态，这里简单取第一条用户记录作为当前用户
            var user = await _db.Users.AsNoTracking().OrderBy(u => u.UserId).FirstOrDefaultAsync();
            if (user == null)
            {
                var guest = new UserDto { Id = Guid.Empty, NickName = "游客", AvatarUrl = "", PhoneNumber = "" };
                return ApiResponse<UserDto>.Ok(guest);
            }

            var dto = new UserDto
            {
                Id = Guid.Empty,
                NickName = user.WxNickname ?? user.UserNo,
                AvatarUrl = user.WxImage ?? "",
                PhoneNumber = user.PhoneNumber ?? ""
            };

            return ApiResponse<UserDto>.Ok(dto);
        }
        #endregion

        // Used by: demo/pages/profile/profile.js (更新用户信息)
        #region Update - demo/pages/profile/profile.js更新用户信息
        [HttpPut]
        public async Task<ActionResult<ApiResponse<object>>> Update([FromBody] UserDto dto)
        {
            var user = await _db.Users.OrderBy(u => u.UserId).FirstOrDefaultAsync();
            if (user == null)
            {
                return ApiResponse<object>.Fail("用户不存在", 404);
            }

            if (!string.IsNullOrWhiteSpace(dto.NickName))
            {
                user.WxNickname = dto.NickName;
            }

            if (!string.IsNullOrWhiteSpace(dto.AvatarUrl))
            {
                user.WxImage = dto.AvatarUrl;
            }

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                user.PhoneNumber = dto.PhoneNumber;
            }

            await _db.SaveChangesAsync();
            return ApiResponse<object>.Ok(null);
        }
        #endregion

        // Used by: demo/pages/profile/profile.js (用户统计)
        #region Statistics - demo/pages/profile/profile.js用户统计
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<object>>> Statistics()
        {
            var orderCount = await _db.Orders.CountAsync();
            var stat = new
            {
                OrderCount = orderCount,
                AdoptedAcres = 0,
                Points = 0
            };
            return ApiResponse<object>.Ok(stat);
        }
        #endregion

        // 收货地址相关
        [HttpGet("address")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AddressDto>>>> GetAddressList()
        {
            var user = await _db.Users.AsNoTracking().OrderBy(u => u.UserId).FirstOrDefaultAsync();
            if (user == null)
            {
                return ApiResponse<IEnumerable<AddressDto>>.Ok(Array.Empty<AddressDto>());
            }

            var list = await _db.ShippingAddresses
                .AsNoTracking()
                .Where(a => a.UserId == user.UserId)
                .OrderBy(a => a.AddressId)
                .Select(a => new AddressDto
                {
                    Id = a.AddressId,
                    Name = a.ContentName,
                    Phone = "", // 数据表中未单独存手机号，可按需扩展
                    Province = a.Province,
                    City = a.City,
                    District = a.MunicipalDistricts,
                    Address = a.HouseNumber,
                    IsDefault = false
                })
                .ToListAsync();

            return ApiResponse<IEnumerable<AddressDto>>.Ok(list);
        }

        [HttpPost("address")]
        public async Task<ActionResult<ApiResponse<object>>> AddAddress([FromBody] AddressDto dto)
        {
            var user = await _db.Users.OrderBy(u => u.UserId).FirstOrDefaultAsync();
            if (user == null)
            {
                return ApiResponse<object>.Fail("用户不存在", 404);
            }

            var entity = new Models.Entities.ShippingAddress
            {
                UserId = user.UserId,
                ContentName = dto.Name ?? "",
                Province = dto.Province ?? "",
                City = dto.City ?? "",
                MunicipalDistricts = dto.District ?? "",
                Town = null,
                HouseNumber = dto.Address ?? ""
            };

            _db.ShippingAddresses.Add(entity);
            await _db.SaveChangesAsync();

            return ApiResponse<object>.Ok(new { id = entity.AddressId });
        }

        [HttpPut("address")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateAddress([FromBody] AddressDto dto)
        {
            var entity = await _db.ShippingAddresses.FirstOrDefaultAsync(a => a.AddressId == dto.Id);
            if (entity == null)
            {
                return ApiResponse<object>.Fail("地址不存在", 404);
            }

            if (!string.IsNullOrWhiteSpace(dto.Name)) entity.ContentName = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Province)) entity.Province = dto.Province;
            if (!string.IsNullOrWhiteSpace(dto.City)) entity.City = dto.City;
            if (!string.IsNullOrWhiteSpace(dto.District)) entity.MunicipalDistricts = dto.District;
            if (!string.IsNullOrWhiteSpace(dto.Address)) entity.HouseNumber = dto.Address;

            await _db.SaveChangesAsync();
            return ApiResponse<object>.Ok(null);
        }

        [HttpDelete("address")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteAddress([FromQuery] int id)
        {
            var entity = await _db.ShippingAddresses.FirstOrDefaultAsync(a => a.AddressId == id);
            if (entity == null)
            {
                return ApiResponse<object>.Fail("地址不存在", 404);
            }

            _db.ShippingAddresses.Remove(entity);
            await _db.SaveChangesAsync();
            return ApiResponse<object>.Ok(null);
        }
    }
}