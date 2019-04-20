using indexReact.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace indexReact.Controllers
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
    }
}