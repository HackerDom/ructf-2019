using MongoDB.Driver;
using SharpGeoAPI.HTTP;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.Storages
{
    class Storage : IStorage
    {
        private readonly IMongoCollection<AgentInfo> agents;

        public Storage(ISettings settings)
        {
            var client = new MongoClient(settings.MongoDBConnectionString);
            var database = client.GetDatabase(settings.MongoDBName);
            agents = database.GetCollection<AgentInfo>(settings.AgentsCollectionName);
        }

        public AgentInfo GetAgent(string agentId)
        {
            return agents.Find(agent => agent.AgentId == agentId).FirstOrDefault();
        }

        public void AddAgent(AgentInfo agentInfo)
        {
            agents.InsertOne(agentInfo);
        }

        public void UpdateAgent(AgentInfo agentInfo)
        {
            agents.ReplaceOne(origin => origin.AgentId == agentInfo.AgentId, agentInfo);
        }

        public void RemoveAgent(AgentInfo agentInfoToDelete)
        {
            agents.DeleteOne(origin => origin.AgentId == agentInfoToDelete.AgentId);
        }
    }
}