using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System.Security.Claims;
using Task8Indentity.Models;

namespace Task8Indentity.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMongoCollection<ApplicationUser> _usersCollection;
        private readonly IJwtService _jwtService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            IMongoDatabase mongoDatabase)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _usersCollection = mongoDatabase.GetCollection<ApplicationUser>("Users");
        }

        public async Task<IdentityResult> RegisterAsync(RegisterModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return result;
            }

            if (!string.IsNullOrEmpty(model.Role))
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _roleManager.CreateAsync(new ApplicationRole { Name = model.Role });
                }
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            return result;
        }

        public async Task<LoginResponse> LoginAsync(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
                return new LoginResponse { Success = false, Message = "Invalid username or password" };

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return new LoginResponse { Success = false, Message = "Invalid username or password" };

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateJwtToken(user, roles);

            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpiry = refreshToken.Expires;
            await _userManager.UpdateAsync(user);

            return new LoginResponse
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken.Token,
                Username = user.UserName!,
                Roles = roles,
                Message = $"Welcome {user.UserName}!, You Are Logged In Successfully."
            };
        }

        public async Task<TokenRefreshResponse> RefreshTokenAsync(TokenRefreshRequest request)
        {
            string accessToken = request.AccessToken;
            string refreshToken = request.RefreshToken;

            var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return new TokenRefreshResponse
                {
                    Success = false,
                    Message = "Invalid access token"
                };
            }

            string username = principal.FindFirstValue("username") ??
                             principal.FindFirstValue(ClaimTypes.Name) ??
                             string.Empty;

            var user = await _userManager.FindByNameAsync(username);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                return new TokenRefreshResponse
                {
                    Success = false,
                    Message = "Invalid refresh token or token expired"
                };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _jwtService.GenerateJwtToken(user, roles);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken.Token;
            user.RefreshTokenExpiry = newRefreshToken.Expires;
            await _userManager.UpdateAsync(user);

            return new TokenRefreshResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                Success = true,
                Message = "Token refreshed successfully"
            };
        }

        public async Task<bool> RevokeTokenAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return false;
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            var users = await _usersCollection.Find(_ => true).ToListAsync();
            return users;
        }

    }
}
