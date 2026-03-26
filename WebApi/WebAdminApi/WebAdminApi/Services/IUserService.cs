using WebAdminApi.DTOs;

namespace WebAdminApi.Services
{
    public interface IUserService
    {
        List<UserListItemDto> GetUserList(string? keyword);
        Task<bool> AddUser(AddUserDto dto);
        Task<bool> EditUser(EditUserDto dto);
        Task<bool> ChangeUserStatus(int userId, string status);
        Task<bool> DeleteUser(int userId);
    }
}