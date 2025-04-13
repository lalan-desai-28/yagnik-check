using Microsoft.AspNetCore.Identity;
using Task8Indentity.Models;

namespace Task8Indentity.Services
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterAsync(RegisterModel model);
        Task<LoginResponse> LoginAsync(LoginModel model);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<TokenRefreshResponse> RefreshTokenAsync(TokenRefreshRequest request);
        Task<bool> RevokeTokenAsync(string username);
    }

}
