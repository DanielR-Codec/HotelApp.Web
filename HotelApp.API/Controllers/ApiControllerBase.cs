using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.API.Controllers
{
    /// <summary>
    /// Base of every API controller. Errors are not caught here: HotelAppExceptionHandler turns them into ProblemDetails.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>Id of the user that owns the JWT (claim NameIdentifier), or 0 if there is none.</summary>
        protected int CurrentUserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        /// <summary>404 in the same ProblemDetails format the exception handler uses.</summary>
        protected ObjectResult NotFoundProblem(string detail) =>
            Problem(statusCode: StatusCodes.Status404NotFound, title: "Recurso no encontrado.", detail: detail);
    }
}
