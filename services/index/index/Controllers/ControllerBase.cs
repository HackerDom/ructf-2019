using index.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace index.Controllers
{
    [Route("api/[controller]")]
    public class ControllerBase:Controller
    {
        protected const string LoginKey = "login";

        protected bool IsSessionValid()
        {
            return !Request.Cookies.TryGetValue("sid", out var sid) ||
                   !Request.Cookies.TryGetValue("login", out var login) ||
                   !SessionManager.ValidateSession(login, sid);
        }

        protected JsonResult ThrowError(string error)
        {
            Response.StatusCode = 400;
            return Json(new { error });
        }
    }
}