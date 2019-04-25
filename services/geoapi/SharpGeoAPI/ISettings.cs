using System;

namespace SharpGeoAPI.HTTP
{
    public interface ISettings
    {
        int ParallelismDegree { get; set; }
        int Port { get; set; }
        string MongoDBConnectionString { get; set; }
        string CollectionName { get; set; }
        string MongoDBName { get; set; }

        string ChunkStorageDirectory { get; set; }
        int ChunkStorageCapacity { get; set; }

        int ChunkSize { get; set; }
        TimeSpan ChunkStorageExpirationTime { get; set; }
        int ChunkWidth { get; set; }
        int ChunkHeight { get; set; }
    }
}