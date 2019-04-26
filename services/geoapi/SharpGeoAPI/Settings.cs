namespace SharpGeoAPI
{
    public class Settings : ISettings
    {
        public int ParallelismDegree { get; set; } = 8;
        public int Port { get; set; } = 9007;
        public string MongoDBConnectionString { get; set; } = "mongodb://localhost:27017";
        public string AgentsCollectionName { get; set; } = "AgentsCollection";
        public string TObjectsCollectionName { get; set; } = "TObjecstCollection";
        public string MongoDBName { get; set; } = "AgentsDB";
    }
}