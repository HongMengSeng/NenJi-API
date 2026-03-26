using WebAdminApi.DBs;
using WebAdminApi.DTOs;
using WebAdminApi.Entities;

//删了管理员用户
namespace WebAdminApi.Services
{
    /// <summary>
    /// 用户服务实现类
    /// 负责用户相关的业务逻辑处理
    /// 当前版本使用内存存储，生产环境应替换为数据库访问
    /// </summary>
    public class UserService : IUserService
    {
        /// <summary>
        /// 用户数据集合（内存存储）
        /// 在实际应用中应替换为数据库操作
        /// </summary>
        //private static readonly List<User> _users = InitializeUsers();

        private readonly AppDbContext _dbContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取用户列表，支持按昵称或手机号搜索
        /// </summary>
        /// <param name="keyword">搜索关键词（可选）</param>
        /// <returns>用户列表DTO集合</returns>
        public List<UserListItemDto> GetUserList(string? keyword)
        {
            // 创建可枚举集合用于链式查询
            var query = from adminuser in _dbContext.AdminStaffs
                        join r in _dbContext.Roles
                        on adminuser.Role equals r.RoleId
                        select new { adminuser, r };
            //如果提供了搜索关键词，则进行模糊查询
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(u =>
                    u.adminuser.NickName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    u.adminuser.Phone.Contains(keyword)
                );
            }


            // 将User实体转换为UserListItemDto，用于API返回
            return query.Select(u => new UserListItemDto
            {
                admin_id = u.adminuser.AdminId,
                phone = u.adminuser.Phone,
                nickname = u.adminuser.NickName,
                register_time = u.adminuser.RegisterTime,
                gender = u.adminuser.Gender ?? "未设置",
                address = u.adminuser.Address ?? "未设置",
                role = u.adminuser.Role,
                status = u.adminuser.Status,
            }).ToList();
        }

        /// <summary>
        /// 添加新用户
        /// </summary>
        /// <param name="dto">新用户数据传输对象</param>
        /// <returns>是否添加成功</returns>
        /// <exception cref="Exception">当手机号已存在时抛出异常</exception>
        public async Task<bool> AddUser(AddUserDto dto)
        {
            // 检查手机号是否已存在
            if (_dbContext.AdminStaffs.Any(u => u.Phone == dto.Phone))
            {
                throw new Exception("手机号已存在");
            }

            // 创建新用户实体
            var newUser = new AdminStaffs
            {
                AdminId = dto.AdminId,
                Phone = dto.Phone,
                NickName = dto.Nickname,
                Gender = dto.Gender,
                Address = dto.Address,
                Role = dto.Role,
                Status = dto.Status,
                //password = "123456", // 实际应加密
            };

            // 将用户添加到集合
            _dbContext.AdminStaffs.Add(newUser);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 编辑现有用户信息
        /// </summary>
        /// <param name="dto">编辑用户数据传输对象</param>
        /// <returns>是否编辑成功</returns>
        /// <exception cref="Exception">当用户不存在时抛出异常</exception>
        public async Task<bool> EditUser(EditUserDto dto)
        {
            // 根据用户ID查找用户
            var user = _dbContext.AdminStaffs.FirstOrDefault(u => u.AdminId == dto.AdminId);
            if (user == null)
            {
                throw new Exception("用户不存在");
            }

            // 更新可修改的字段（仅当值不为空时才更新）
            if (!string.IsNullOrWhiteSpace(dto.Nickname))
                user.NickName = dto.Nickname;
            if (!string.IsNullOrWhiteSpace(dto.Gender))
                user.Gender = dto.Gender;
            if (!string.IsNullOrWhiteSpace(dto.Address))
                user.Address = dto.Address;
            if (dto.Role > 0)
                user.Role = dto.Role;
            if (!string.IsNullOrWhiteSpace(dto.Status))
                user.Status = dto.Status;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 更改用户状态（启用/禁用）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="status">新状态</param>
        /// <returns>是否更新成功</returns>
        /// <exception cref="Exception">当用户不存在时抛出异常</exception>
        public async Task<bool> ChangeUserStatus(int userId, string status)
        {
            // 根据用户ID查找用户
            var user = _dbContext.AdminStaffs.FirstOrDefault(u => u.AdminId == userId);
            if (user == null)
            {
                throw new Exception("用户不存在");
            }

            // 更新用户状态
            user.Status = status;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 删除指定用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>是否删除成功</returns>
        /// <exception cref="Exception">当用户不存在时抛出异常</exception>
        public async Task<bool> DeleteUser(int userId)
        {
            // 根据用户ID查找用户
            var user = _dbContext.AdminStaffs.FirstOrDefault(u => u.AdminId == userId);
            if (user == null)
            {
                throw new Exception("用户不存在");
            }

            // 从集合中移除用户
            _dbContext.AdminStaffs.Remove(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 生成用户ID
        /// 格式：U + 日期(yyyyMMdd) + 序号(6位)
        /// 例如：U202601010000001
        /// </summary>
        /// <returns>新生成的用户ID</returns>
        //private string GenerateUserId()
        //{
        //    var date = DateTime.Now.ToString("yyyyMMdd");
        //    var sequence = _dbContext.AdminStaffs.Count(u => u.AdminId.StartsWith($"U{date}")) + 1;
        //    return $"U{date}{sequence:D6}";
        //}
    }
}
