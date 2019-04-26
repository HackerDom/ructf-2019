using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SharpGeoAPI.Models;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI.HTTP.Handlers
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

            var tObject = new TerrainObject(request.AgentId, GetAgentToken())
            {
                Info = request.Info,
                Cells = request.Cells,
            };

            terrainObjectStore.UploadTerrainObject(agentInfo.AgentToken, GetAgentToken(), tObject);

            await context.Response.Send(200, tObject.IndexKey);
        }

        private string GetAgentToken()
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[settings.ObjectIdSize];
                rng.GetBytes(tokenData);

                return Convert.ToBase64String(tokenData);
            }
        }

        private class UploadObjectRequest
        {
            public string AgentId { get; set; }
            public byte[,] Cells { get; set; }
            public string Info { get; set; }
        }
    }
}
 