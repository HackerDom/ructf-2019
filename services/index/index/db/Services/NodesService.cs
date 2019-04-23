using index.db.Models;
using Microsoft.Extensions.Configuration;

namespace index.db.Services
{
    public class NodesService : ServiceBase<Node>
    {
        public NodesService(IConfiguration config) : base(config, "nodes")
        {
        }
    }
}