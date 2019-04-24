using System;
using System.Threading.Tasks;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.Storages
{
    public interface IAgentStorage : IDisposable
    {
        Task<Agent> GetAgent(string sessionId);
        Task AddAgent(Agent agent);
    }
}