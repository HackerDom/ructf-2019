using System.Threading.Tasks;
using LiteDB;
using SharpGeoAPI.Models;
using SharpGeoAPI.Models.Geo;

namespace SharpGeoAPI.Storages
{
    class AgentsStorage : IAgentStorage
    {
        private LiteDatabase db;
        protected readonly LiteCollection<Agent> sessions;

        public AgentsStorage()
        {
            db = new LiteDatabase(DBNames.DBName);
            sessions = db.GetCollection<Agent>(DBNames.SessionCollection);
        }

        public async Task<Agent> GetAgent(string sessionId)
        {
            return sessions.FindById(sessionId);
        }

        public async Task AddAgent(Agent agent)
        {
            sessions.Insert(agent.AgentId, agent);
        }

      
        public void Dispose()
        {
            db.Dispose();
        }

        private class DBNames
        {
            public static string SessionCollection => "SessionsCollection";
            public static string DBName => "Seeds.db";
        }
    }

}