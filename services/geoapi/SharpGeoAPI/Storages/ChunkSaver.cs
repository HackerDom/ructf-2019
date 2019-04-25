using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using SharpGeoAPI.HTTP;
using SharpGeoAPI.Models.Geo;

namespace SharpGeoAPI.Storages
{
    public class ChunksGroup
    {
        private readonly SemaphoreSlim semaphore;

        public ChunksGroup(int index, byte[] data)
        {
            Index = index;
            this.Data = data;
        }

        public readonly int Index;
        public readonly byte[] Data;

        public string GetFileName() => $"{Index}_Chunks";
    }

    public class ChunkSaver : IChunkSaver
    {
        private readonly ILog log;
        private readonly string path;
        private readonly BinaryFormatter binaryFormatter = new BinaryFormatter();


        private ChunkSaver(ISettings settings, ILog log)
        {
            this.log = log;
            this.path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settings.ChunkStorageDirectory);
        }

        public void Save(ChunksGroup chunksGroup)
        {
            try
            {
                var tempFileName = $"{chunksGroup.GetFileName()}.tmp";
                using (var fs = new FileStream(tempFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    try
                    {
                        binaryFormatter.Serialize(fs, chunksGroup);
                    }
                    catch (SerializationException e)
                    {
                        log.Warn($"Exception while saving file '{path}.tmp'", e);
                    }
                }

                SwapFiles(chunksGroup.GetFileName(), tempFileName);
            }
            catch (Exception ex)
            {
                log.Warn($"Exception while saving file '{path}'", ex);
            }
        }

        private void SwapFiles(string originFileName, string newFileName)
        {
            if (File.Exists(path)) File.Delete(originFileName);
            File.Move(newFileName, originFileName);
        }


        public async Task<IEnumerable<ChunksGroup>> LoadChunksPart()
        {
            var formatter = new BinaryFormatter();
            try
            {
                var bytes = await File.ReadAllBytesAsync(path);
                using (var serializationStream = new MemoryStream(bytes))
                {
                    //TODO: Fix it!
                    throw new NotImplementedException();
                     ///return (ChunksGroup)formatter.Deserialize(serializationStream);
                }
            }
            catch (FileNotFoundException e)
            {
                log.Warn($"DB file doesn't exist!");
                return null;
            }
            catch (SerializationException e)
            {
                log.Warn($"Failed to deserialize. Reason: {e.Message}");
                return null;
            }
        }

    }
}