using System;
using System.Security.Cryptography;
using System.Text;
using indexReact.db;
using indexReact.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace indexReact.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IServiceBase<User> userDb;

        public UsersController(IServiceBase<User> userDb)
        {
            this.userDb = userDb;
        }

        [HttpGet("validate")]
        public JsonResult ValidateSession()
        {
            if (!Request.Cookies.TryGetValue("sid", out var sid) ||
                !Request.Cookies.TryGetValue("login", out var login) ||
                !SessionManager.ValidateSession(login, sid))
                return Json(null);

            return Json(login);
        }

        [HttpPost("register")]
        public void CreateUser()
        {
            var formCollection = Request.Form;
            formCollection.TryGetValue("login", out var login);
            formCollection.TryGetValue("pwd", out var password);
            userDb.Create(Create(login, password));
            var sid = SessionManager.CreateSession(login);
            Response.Cookies.Append("sid", sid);
            Response.Cookies.Append("login", login);
        }

        [HttpPost("logout")]
        public void LogOut()
        {
            Response.Cookies.Delete("sid");
            Response.Cookies.Delete("login");
        }

        private static User Create(string login, string password)
        {
            using (var sha512 = SHA512.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var pwdHash = Convert.ToBase64String(sha512.ComputeHash(passwordBytes));

                return new User
                {
                    Login = login,
                    PwdHash = pwdHash
                };
            }
        }
    }
}