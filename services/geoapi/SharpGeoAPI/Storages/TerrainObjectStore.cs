using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.Storages
{
    public class TerrainObjectStore : ITerrainObjectStore
    {
        private readonly IMongoCollection<TerrainObject> terrainObjects;
        private readonly ISettings settings;

        public TerrainObjectStore(ISettings settings)
        {
            this.settings = settings;
            var client = new MongoClient(settings.MongoDBConnectionString);
            var database = client.GetDatabase(settings.MongoDBName);
            terrainObjects = database.GetCollection<TerrainObject>(settings.TObjectsCollectionName);
        }

        public TerrainObject GetTerrainObject(string agentName, string objectId)
        {
            return terrainObjects.Find(GetKey(agentName, objectId)).FirstOrDefault();
        }

        public async Task<IEnumerable<TerrainObject>> GetTerrainObject(string agentName)
        {
            var prefix = Builders<TerrainObject>.Filter.Regex("x", new BsonRegularExpression(agentName, "i"));
           // return await terrainObjects.Find(prefix).Limit(100).ToListAsync()
           throw new NotImplementedException();
        }

        public void UploadTerrainObject(string agentName, string objectId, TerrainObject terrainObject)
        {
            terrainObjects.InsertOne(terrainObject);
        }

        public static string GetKey(string agentName, string objectId)
        {
            return $"{agentName}{objectId}";
        }
    }
}
