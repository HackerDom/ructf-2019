using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SharpGeoAPI.Models.Geo
{
    public class Chunk : IChunk
    {
        private readonly int width;
        private readonly int height;
        private readonly byte[] cells;


        private readonly SemaphoreSlim semaphore;
        private readonly BlockingCollection<Tile> tileChangeQueue;

        public Chunk(int width, int height, byte[] cells)
        {
            this.width = width;
            this.height = height;
            this.cells = cells;

            semaphore = new SemaphoreSlim(1,1 );

            tileChangeQueue = new BlockingCollection<Tile>(new ConcurrentQueue<Tile>(), 100);
        }

        public Chunk(int width, int height) : this(width, height, new byte[width * height])
        {
        }

        public bool TrySet(Vector2 point, CellType cell)
        {
            return tileChangeQueue.TryAdd(new Tile(point, cell));
        }

        public async Task Update()
        {
            await semaphore.WaitAsync();
            foreach (var tile in tileChangeQueue.GetConsumingEnumerable())
            {
                cells[CoordsToIndex(tile.position.X, tile.position.Y)] = (byte) tile.Cell;
            }
            semaphore.Release();
        }

        public async Task HandleSnapshot(Func<byte[], Task> action)
        {
            await semaphore.WaitAsync();
            await action(cells);
            semaphore.Release();
        }

        private int CoordsToIndex(int x, int y)
        {
            return y * width + x;
        }
    }
}