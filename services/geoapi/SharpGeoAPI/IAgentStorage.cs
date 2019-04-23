using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpGeoAPI.HTTP.Handlers;
using SharpGeoAPI.Models;

namespace SharpGeoAPI
{
    public interface IAgentStorage : IDisposable
    {
        Task<Agent> GetAgent(string sessionId);
        Task AddAgent(Agent agent);
    }
}