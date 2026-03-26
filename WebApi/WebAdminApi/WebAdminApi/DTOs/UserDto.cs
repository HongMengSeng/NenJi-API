namespace WebAdminApi.DTOs
{
    /// <summary>
    /// 用户列表响应DTO
    /// </summary>
    public class UserListItemDto
    {
        public int admin_id { get; set; }
        public string phone { get; set; } = null!;
        public string nickname { get; set; } = null!;
        public string gender { get; set; } = null!;
        public string address { get; set; } = null!;
        public int role { get; set; }
        public string status { get; set; } = null!;

        public DateTime register_time = DateTime.Now;
    }

    /// <summary>
    /// 新增用户请求DTO
    /// </summary>
    public class AddUserDto
    {
        public int AdminId { get; set; }
        public string Phone { get; set; } = "未设置";
        public string Nickname { get; set; } = "未设置";
        public string Gender { get; set; } = "未设置";
        public string Address { get; set; } = "未设置";
        public int Role { get; set; }
        public string Status { get; set; } = "禁用";
    }

    /// <summary>
    /// 编辑用户请求DTO
    /// </summary>
    public class EditUserDto
    {
        public int AdminId { get; set; } 
        public string Phone { get; set; } = "未设置";
        public string Nickname { get; set; } = "未设置";
        public string Gender { get; set; } = "未设置";
        public string Address { get; set; } = "未设置";
        public int Role { get; set; }
        public string Status { get; set; } = "禁用";
    }

    /// <summary>
    /// 修改用户状态请求DTO
    /// </summary>
    public class ChangeStatusDto
    {
        public int Id { get; set; }
        public int Status { get; set; }
    }

    /// <summary>
    /// 删除用户请求DTO
    /// </summary>
    public class DeleteUserDto
    {
        public string Id { get; set; } = null!;
    }
}