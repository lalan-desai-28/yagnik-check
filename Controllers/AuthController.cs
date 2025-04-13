using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Task8Indentity.Models;
using Task8Indentity.Services;

namespace Task8Indentity.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        /// <summary>
        /// Register a new user in the system.
        /// </summary>
        /// <param name="model">The user registration model containing username, email, password, and role.</param>
        /// <returns>Returns a success message upon successful registration, or validation errors if any.</returns>
        [HttpPost("register")]
        [SwaggerResponse(200, "User registered successfully.")]
        [SwaggerResponse(400, "Bad Request: Validation errors.")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var result = await _authService.RegisterAsync(model);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("User Registered Successfully: " + model);
        }

        /// <summary>
        /// Login an existing user and generate a JWT token and refresh token.
        /// </summary>
        /// <param name="model">The login model containing username and password.</param>
        /// <returns>A JWT token and refresh token if the login is successful, or an error message if the login fails.</returns>
        [HttpPost("login")]
        [SwaggerResponse(200, "Login successful. Returns JWT token and refresh token.")]
        [SwaggerResponse(401, "Unauthorized: Invalid username or password.")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var response = await _authService.LoginAsync(model);
            if (!response.Success)
            {
                return Unauthorized(response.Message);
            }

            return Ok(response);
        }

        /// <summary>
        /// Refresh the JWT token using a valid refresh token.
        /// </summary>
        /// <param name="request">The token refresh request containing the expired access token and refresh token.</param>
        /// <returns>A new JWT token and refresh token if the refresh is successful, or an error message if it fails.</returns>
        [HttpPost("refresh-token")]
        [SwaggerResponse(200, "Token refreshed successfully. Returns new JWT token and refresh token.")]
        [SwaggerResponse(400, "Bad Request: Invalid token.")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRefreshRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }

            return Ok(response);
        }

        /// <summary>
        /// Revoke the refresh token for the current user.
        /// </summary>
        /// <returns>Success message if the token was successfully revoked, or an error message if it fails.</returns>
        [HttpPost("revoke")]
        [Authorize]
        [SwaggerResponse(200, "Token revoked successfully.")]
        [SwaggerResponse(401, "Unauthorized: User not found.")]
        public async Task<IActionResult> RevokeToken()
        {
            var username = User.FindFirstValue("username") ??
                         User.FindFirstValue(ClaimTypes.Name) ??
                         string.Empty;

            var result = await _authService.RevokeTokenAsync(username);
            if (!result)
            {
                return Unauthorized("Invalid user");
            }

            return Ok("Token revoked successfully");
        }
    }
}