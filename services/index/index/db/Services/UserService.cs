using index.db.Models;
using Microsoft.Extensions.Configuration;

namespace index.db.Services
{
    public class UserService : ServiceBase<User>
    {
        public UserService(IConfiguration config) : base(config, "users")
        {
        }
    }
}