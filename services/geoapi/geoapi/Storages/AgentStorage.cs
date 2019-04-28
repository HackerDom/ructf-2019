using System;
using geoapi.Models;
using geoapi.Utils;
using MongoDB.Driver;

namespace geoapi.Storages
{
    class AgentStorage : IAgentStorage
    {
        private readonly IMongoCollection<AgentInfo> agents;

        public AgentStorage(ISettings settings)
        {
            var client = new MongoClient(settings.MongoDBConnectionString);
            var database = client.GetDatabase(settings.MongoDBName);
            agents = database.GetCollection<AgentInfo>(settings.AgentsCollectionName);
            agents.Indexes.CreateOneAsync(Builders<AgentInfo>.IndexKeys.Ascending(_ => _.AgentToken)).GetAwaiter().GetResult();
            agents.Indexes.CreateOne(Builders<AgentInfo>.IndexKeys.Ascending("expireAt"), new CreateIndexOptions { ExpireAfter = TimeSpan.FromSeconds(30) });
        }

        public AgentInfo GetAgent(string agentId)
        {
            return agents.Find(agent => agent.AgentToken == agentId).FirstOrDefault();
        }

        public void AddAgent(AgentInfo agentInfo)
        {
            agents.InsertOne(agentInfo);
        }
    }
}