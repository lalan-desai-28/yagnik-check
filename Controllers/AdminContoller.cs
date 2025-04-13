using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Task8Indentity.Controllers
{
  [Route("api/admin")]
  [ApiController]
  public class AdminContoller : ControllerBase
  {

    /// <summary>
    /// This endpoint is accessible only by users with the "Admin" role.
    /// </summary>
    /// <returns>A success message indicating the endpoint is accessible only by Admins.</returns>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [SwaggerResponse(200, "This is an admin-only endpoint.")]
    [SwaggerResponse(403, "Forbidden: The user does not have the required role.")]
    public IActionResult AdminOnlyEndpoint()
    {
      return Ok("This is an admin-only endpoint.");
    }

  }
}
