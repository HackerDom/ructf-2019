using System.Net;
using System.Threading.Tasks;
using geoapi.Models;
using geoapi.Storages;
using geoapi.Utils;

namespace geoapi.HTTP.Handlers
{
    public class UploadTerrainObjectHandler : BaseHandler
    {
        private readonly IStorage storage;
        private readonly ITerrainObjectStore terrainObjectStore;
        private readonly ISettings settings;


        public UploadTerrainObjectHandler(IStorage storage, ITerrainObjectStore terrainObjectStore, ISettings settings) : base("PUT", "object")
        {
            this.storage = storage;
            this.terrainObjectStore = terrainObjectStore;
            this.settings = settings;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContentAsync();

            var request = content.FromJson<UploadObjectRequest>();

            var agentInfo = storage.GetAgent(request.AgentId);

            if (agentInfo == null)
            {
                await context.Response.Send(404, "Can't find session");
                return;
            }

            var tObject = new TerrainObject(request.AgentId, GenerateId(settings.ObjectIdSize))
            {
                Info = request.Info,
                Cells = request.Cells,
            };

            terrainObjectStore.UploadTerrainObject(tObject);

            await context.Response.Send(200, tObject.IndexKey);
        }



        private class UploadObjectRequest
        {
            public string AgentId { get; set; }
            public byte[,] Cells { get; set; }
            public string Info { get; set; }
        }
    }
}
 