using indexReact.db.Models;
using Microsoft.Extensions.Configuration;

namespace indexReact.db.Services
{
    public class UserService : ServiceBase<User>
    {
        public UserService(IConfiguration config) : base(config, "users")
        {
        }
    }
}