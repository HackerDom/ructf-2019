using Microsoft.AspNetCore.Mvc;

namespace indexReact.Controllers
{
    [Route("api/[controller]")]
    public class UsersController: Controller
    {
        [HttpPost("register")]
        public void CreateUser()
        {
            //create user

            var formCollection = Request.Form;
            var tryGetValue = formCollection.TryGetValue("login", out var login);
            SessionManager.CreateSession(login);
        }
    }
}