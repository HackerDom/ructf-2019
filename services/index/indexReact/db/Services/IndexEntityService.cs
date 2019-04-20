using indexReact.db.Models;
using Microsoft.Extensions.Configuration;

namespace indexReact.db.Services
{
    public class IndexEntityService : ServiceBase<IndexEntity>
    {
        protected IndexEntityService(IConfiguration config) : base(config, "index")
        {
        }
    }
}