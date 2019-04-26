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

            terrainObjects.Indexes.CreateOneAsync(Builders<TerrainObject>.IndexKeys.Ascending(_ => _.IndexKey)).GetAwaiter().GetResult();
        }

        public TerrainObject GetTerrainObject(string objectId)
        {
            return terrainObjects.Find(tobjcet => tobjcet.IndexKey == objectId).FirstOrDefault();
        }

        public IEnumerable<TerrainObject> GetTerrainObjects(string agentName)
        {
            return terrainObjects.Find(tObject => tObject.IndexKey.StartsWith(agentName)).Limit(settings.SearchLimit).ToList();
        }

        public void UploadTerrainObject(string agentName, string objectId, TerrainObject terrainObject)
        {
            terrainObjects.InsertOne(terrainObject);
        }
    }
}
