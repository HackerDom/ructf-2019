using System;
using System.Threading.Tasks;
using SharpGeoAPI.Models.Geo;

namespace SharpGeoAPI.Storages
{
    class AgentController : IAgentController
    {
        private readonly IStorage storage;
        private readonly IChunkCache chunkCache;

        public AgentController(IStorage storage, IChunkCache chunkCache)
        {
            this.storage = storage;
            this.chunkCache = chunkCache;
        }

        public void ChangeTileType(string agentId, Vector2 position, CellType cellType)
        {
            var agent = storage.GetAgent(agentId);
            var chunk = chunkCache.GetChunk(agent.AgentKey);
            chunk.Set(position ,cellType);
        }
    }


    public interface IChunkCache
    {
        Chunk GetChunk(string agentName);
        Task SaveChunk(string agentName, Chunk chunk);
    }

    public class ChunkCache : IChunkCache
    {
        public Chunk GetChunk(string agentName)
        {
            throw new NotImplementedException();
        }

        public Task SaveChunk(string agentName, Chunk chunk)
        {
            throw new NotImplementedException();
        }
    }
}