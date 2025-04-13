using System.Security.Claims;
using Task8Indentity.Models;

namespace Task8Indentity.Services
{
    public interface IJwtService
    {
        string GenerateJwtToken(ApplicationUser user, IList<string> roles);
        RefreshToken GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }


}
