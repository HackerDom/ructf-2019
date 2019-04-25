using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGeoAPI.HTTP;
using SharpGeoAPI.Models.Geo;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI.Logic
{
    public class ChunkManager : IChunkManager
    {
        private readonly ConcurrentDictionary<int, byte[]> chunksGroups;
        private readonly ISettings settings;
        private readonly IChunkSaver chunkSaver;

        public ChunkManager(ISettings settings, IChunkSaver chunkSaver)
        {
            this.settings = settings;
            this.chunkSaver = chunkSaver;
            chunksGroups = new ConcurrentDictionary<int, byte[]>();
        }

        public Chunk GetChunk(int chunkGroup, int chunkIndex)
        {
            throw new NotImplementedException();
        }

    }
}
