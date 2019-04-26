namespace SharpGeoAPI
{
    public interface ISettings
    {
        int ParallelismDegree { get; set; }
        int Port { get; set; }
        string MongoDBConnectionString { get; set; }
        string AgentsCollectionName { get; set; }
        string MongoDBName { get; set; }
        string TObjectsCollectionName { get; set; }

        int AgentIdSize { get; set; }
        int ObjectIdSize { get; set; }
        int SearchLimit { get; set; }
    }
}