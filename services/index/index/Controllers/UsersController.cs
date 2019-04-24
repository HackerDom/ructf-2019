using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using index.db.Models;
using index.db.Services;
using index.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace index.Controllers
{
    public class UsersController : ControllerBase
    {
        private readonly IServiceBase<User> userDb;

        public UsersController(IServiceBase<User> userDb)
        {
            this.userDb = userDb;
        }

        [HttpGet("validate")]
        public JsonResult ValidateSession()
        {
            if (IsSessionNotValid())
                return Json(null);

            Request.Cookies.TryGetValue(LoginKey, out var login);
            return Json(login);
        }

        [HttpPost("register")]
        public ActionResult CreateUser()
        {
            var formCollection = Request.Form;
            formCollection.TryGetValue(LoginKey, out var login);
            formCollection.TryGetValue("pwd", out var password);

            var user = userDb.Get().FirstOrDefault(u => u.Login == login);
            if (user != null)
            {
                var pwdHash = GetPwdHash(password);
                if (user.PwdHash != pwdHash)
                    return StatusCode(401);
            }
            else
            {
                userDb.Create(Create(login, password));
            }

            var sid = SessionManager.CreateSession(login);
            Response.Cookies.Append("sid", sid);
            Response.Cookies.Append(LoginKey, login);
            return StatusCode(201);
        }

        [HttpPost("logout")]
        public void LogOut()
        {
            Response.Cookies.Delete("sid");
            Response.Cookies.Delete(LoginKey);
        }

        private static User Create(string login, string password)
        {
            return new User
            {
                Login = login,
                PwdHash = GetPwdHash(password)
            };
        }

        private static string GetPwdHash(string password)
        {
            using (var sha512 = SHA512.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                return Convert.ToBase64String(sha512.ComputeHash(passwordBytes));
            }
        }
    }
}