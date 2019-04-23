using indexReact.db.Models;
using Microsoft.Extensions.Configuration;

namespace indexReact.db.Services
{
    public class NodesService : ServiceBase<Node>
    {
        public NodesService(IConfiguration config) : base(config, "nodes")
        {
        }
    }
}