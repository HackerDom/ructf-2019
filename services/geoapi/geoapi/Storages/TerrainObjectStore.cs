using System;
using System.Collections.Generic;
using MongoDB.Driver;
using SharpGeoAPI.Models;
using SharpGeoAPI.Utils;

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
            terrainObjects.Indexes.CreateOne(Builders<TerrainObject>.IndexKeys.Ascending("expireAt"), new CreateIndexOptions { ExpireAfter = new TimeSpan(1, 30, 0) });
        }

        public TerrainObject GetTerrainObject(string objectId)
        {
            return terrainObjects.Find(tobjcet => tobjcet.IndexKey == objectId).FirstOrDefault();
        }

        public IEnumerable<TerrainObject> GetTerrainObjects(string agentName, int skip, int take)
        {
            return terrainObjects.Find(tObject => tObject.IndexKey.StartsWith(agentName))
                    .Skip(skip)
                    .Limit(Math.Max(take - skip, settings.SearchLimit))
                    .ToList();
        }

        public void UploadTerrainObject(TerrainObject terrainObject)
        {
            terrainObjects.InsertOne(terrainObject);
        }
    }
}
