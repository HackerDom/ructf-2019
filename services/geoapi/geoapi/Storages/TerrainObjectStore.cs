using System;
using System.Collections.Generic;
using geoapi.Models;
using geoapi.Utils;
using MongoDB.Driver;

namespace geoapi.Storages
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
            terrainObjects.Indexes.CreateOne(Builders<TerrainObject>.IndexKeys.Ascending("expireAt"), new CreateIndexOptions { ExpireAfter = settings.TTL });
        }

        public TerrainObject GetTerrainObject(string objectId)
        {
            return terrainObjects.Find(tobjcet => tobjcet.IndexKey == objectId).FirstOrDefault();
        }

        public IEnumerable<TerrainObject> GetTerrainObjects(string agentToken, int skip, int take)
        {
            return terrainObjects.Find(tObject => tObject.IndexKey.StartsWith(agentToken))
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
