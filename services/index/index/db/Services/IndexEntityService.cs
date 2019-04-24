using index.db.Models;
using Microsoft.Extensions.Configuration;

namespace index.db.Services
{
    public class IndexEntityService : ServiceBase<IndexEntity>
    {
        public IndexEntityService(IConfiguration config) : base(config, "index")
        {
        }
    }
}