using geoapi2.Models;

namespace geoapi2.Storages
{
    public interface IAgentStorage
    {
        AgentInfo GetAgent(string agentId);
        void AddAgent(AgentInfo agentInfo);
    }
}