using System;

namespace geoapi.Utils
{
    public class Settings : ISettings
    {
        public int ParallelismDegree { get; set; } = 100;
        public int Port { get; set; } = 9007;
        public string MongoDBConnectionString { get; set; } = "mongodb://localhost:27017";
        public string AgentsCollectionName { get; set; } = "AgentsCollection";
        public string TObjectsCollectionName { get; set; } = "TObjecstCollection";
        public int MaxContentLength { get; set; }
        public int AgentIdSize { get; set; } = 12;
        public int ObjectIdSize { get; set; } = 12;
        public int SearchLimit { get; set; } = 100;
        public TimeSpan TTL { get; set; } = new TimeSpan(0, 0, 30);
        public string MongoDBName { get; set; } = "AgentsDB";
    }
}