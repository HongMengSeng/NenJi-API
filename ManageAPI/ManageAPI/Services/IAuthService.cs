using ManageAPI.Dtos;

namespace ManageAPI.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginByDeviceAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginByWechatAsync(WechatLoginRequest request, CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginByPhoneAsync(int? currentUserId, PhoneLoginRequest request, CancellationToken cancellationToken = default);

    Task<AuthUserDto?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default);
}
