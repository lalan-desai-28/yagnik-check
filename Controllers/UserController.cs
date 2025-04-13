using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Task8Indentity.Services;

namespace Task8Indentity.Controllers
{
  [Authorize(Roles = "Admin,User")]
  [Route("api/user")]
  [ApiController]
  public class UserController : ControllerBase
  {

    private readonly IAuthService _authService;

    public UserController(IAuthService authService)
    {
      _authService = authService;
    }


    /// <summary>
    /// This endpoint is accessible by users with either the "Admin" or "User" role.
    /// </summary>
    /// <returns>A success message indicating the endpoint is accessible by Admins and Users.</returns>
    [HttpGet("user-or-admin")]
    [SwaggerResponse(200, "This endpoint is accessible by Admins and Users.")]
    [SwaggerResponse(403, "Forbidden: The user does not have the required role.")]
    public IActionResult UserOrAdminEndpoint()
    {
      return Ok("This endpoint is accessible by Admins and Users.");
    }

    /// <summary>
    /// Get all registered users (accessible by Admin only).
    /// </summary>
    /// <returns>A list of registered users.</returns>
    [HttpGet("all-users")]
    [SwaggerResponse(200, "List of all registered users.")]
    [SwaggerResponse(403, "Forbidden: The user does not have the required role.")]
    public async Task<IActionResult> GetAllUsers()
    {
      var users = await _authService.GetAllUsersAsync();
      if (users == null || !users.Any())
      {
        return NotFound("No users found.");
      }

      return Ok(users);
    }
  }
}