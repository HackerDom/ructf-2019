using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpGeoAPI.HTTP;
using SharpGeoAPI.Models.Geo;

namespace SharpGeoAPI.Storages
{
    internal class ChunkStorage
    {
        private readonly string storageFile;
        private readonly int chunkSize;


        public ChunkStorage(string storageName, Settings settings)
        {
            storageFile = Path.Combine(settings.ChunkStorageDirectory, storageName);
            chunkSize = settings.ChunkSize;
        }

        public async Task SaveChunk(Chunk chunk, int index)
        {
            await semaphore.WaitAsync();

            using (var fileStream = new FileStream(storageFile, FileMode.OpenOrCreate))
            {
                await fileStream.WriteAsync(chunk.Cells, index* chunkSize, chunkSize);
            }

            nextIndex++;
            semaphore.Release();

        }

        public async Task<Chunk> GetChunk(int index)
        {
            try
            {
                await semaphore.WaitAsync();

                if (nextIndex <= index)
                {
                    return null;
                }

                var chunkBytes = new byte[chunkSize];
                using (var fileStream = new FileStream(storageFile, FileMode.Open))
                {
                    await fileStream.ReadAsync(chunkBytes, index*chunkSize, chunkSize);
                }

                return new Chunk(chunkBytes);
            }
            catch (Exception e)
            {
            }
            finally
            {
                semaphore.Release();
            }

            return null;
        }

        public void RemoveChunkStorage()
        {
            File.Delete(storageFile);
        }


        private int nextIndex = 0;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    }

}
