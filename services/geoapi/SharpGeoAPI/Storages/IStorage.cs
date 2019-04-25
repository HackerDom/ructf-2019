using System;
using System.Threading.Tasks;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.Storages
{
    public interface IStorage
    {
        AgentSession GetAgent(string agentId);
        void AddAgent(AgentSession agentSession);
    }
}