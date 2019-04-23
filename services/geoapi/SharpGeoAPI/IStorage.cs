using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpGeoAPI.HTTP.Handlers;
using SharpGeoAPI.Models;

namespace SharpGeoAPI
{
    public interface IStorage : IDisposable
    {
        Task<Session> GetSession(string sessionId);
        Task PutSession(Session session);
        Task PutSeed(string sessionId, Seed seed);
        Task<IEnumerable<Seed>> GetSeeds(string sessionId);
        Task<Seed> GetSeed(string sessionId, string seedName);
    }
}