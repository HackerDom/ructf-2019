using index.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace index.Controllers
{
    [Route("api/[controller]")]
    public class ControllerBase : Controller
    {
        protected const string LoginKey = "login";

        protected bool IsSessionNotValid()
        {
            return !Request.Cookies.TryGetValue("sid", out var sid) ||
                   !Request.Cookies.TryGetValue(LoginKey, out var login) ||
                   !SessionManager.ValidateSession(login, sid);
        }

        protected string GetLogin()
        {
            Request.Cookies.TryGetValue(LoginKey, out var login);
            return login;
        }

        protected JsonResult ThrowError(string error)
        {
            Response.StatusCode = 400;
            return Json(new { error });
        }
    }
}