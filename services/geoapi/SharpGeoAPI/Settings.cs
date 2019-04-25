using System;

namespace SharpGeoAPI.HTTP
{
    public class Settings : ISettings
    {
        public int ParallelismDegree { get; set; } = 8;
        public int Port { get; set; } = 9007;
        public string MongoDBConnectionString { get; set; } = "mongodb://localhost:27017";
        public string CollectionName { get; set; } = "geoAPIDB";
        public string MongoDBName { get; set; } = "AgentsDB";
        public TimeSpan ActionHandlerLifeTime { get; set; } = TimeSpan.FromSeconds(10);
        public int ActionQueueMaxSize { get; set; } = 10;


        public string ChunkStorageDirectory { get; set; }  = Environment.CurrentDirectory;
        public int ChunkStorageCapacity { get; set; } = 10;
        public int ChunkSize { get; set; } = 64*64;
        public TimeSpan ChunkStorageExpirationTime { get; set; } = TimeSpan.FromMinutes(1);

        public int ChunkWidth { get; set; }
        public int ChunkHeight { get; set; }
    }
}