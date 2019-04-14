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

        [HttpPost("register")]
        public void CreateUser()
        {
            //create user

            var formCollection = Request.Form;
            formCollection.TryGetValue("login", out var login);
            formCollection.TryGetValue("pwd", out var password);
            userDb.Create(Create(login, password));
            SessionManager.CreateSession(login);
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