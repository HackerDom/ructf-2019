using System;
using System.Net;
using System.Threading.Tasks;
using geoapi.Models;
using geoapi.Storages;
using geoapi.Utils;

namespace geoapi.HTTP.Handlers
{
    public class UploadTerrainObjectHandler : BaseHandler
    {
        private readonly IAgentStorage agentStorage;
        private readonly ITerrainObjectStore terrainObjectStore;
        private readonly ISettings settings;


        public UploadTerrainObjectHandler(IAgentStorage agentStorage, ITerrainObjectStore terrainObjectStore, ISettings settings) : base("PUT", "object")
        {
            this.agentStorage = agentStorage;
            this.terrainObjectStore = terrainObjectStore;
            this.settings = settings;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContentAsync();
            
            //context.Request.InputStream.

            var request = content.FromJson<UploadObjectRequest>();


            var agentInfo = agentStorage.GetAgent(request.AgentId);

            if (agentInfo == null)
            {
                await context.Response.Send(404, "Can't find session");
                return;
            }

            var tObject = new TerrainObject(request.AgentId, GenerateId(settings.ObjectIdSize), DateTime.UtcNow + settings.TTL)
            {
                Info = request.Info,
                Cells = request.Cells,
            };

            terrainObjectStore.UploadTerrainObject(tObject);

            await context.Response.Send(200, tObject.IndexKey);
        }


    }
}
 