using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpGeoAPI.Storages
{
    public interface IChunkSaver
    {
        void Save(ChunksGroup chunksGroup);
        Task<IEnumerable<ChunksGroup>> LoadChunksPart();
    }
}