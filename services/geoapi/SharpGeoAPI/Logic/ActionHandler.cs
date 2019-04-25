using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using SharpGeoAPI.HTTP;

namespace SharpGeoAPI.Logic
{
    public class ActionHandler
    {
        private readonly BlockingCollection<Func<Task>> taskQueue;
        private readonly TimeSpan timeBudget;
        private DateTime lastUsedAt;

        public ActionHandler(Settings settings)
        {
            lastUsedAt = DateTime.UtcNow;
            timeBudget = settings.ActionHandlerLifeTime;
            taskQueue = new BlockingCollection<Func<Task>>(new ConcurrentQueue<Func<Task>>(),
                settings.ActionQueueMaxSize);

            Task.Run(StartProcess);
        }

        public async Task Stop()
        {
            taskQueue.CompleteAdding();
            await WaitForCompletion();
        }

        private async Task WaitForCompletion()
        {
            while (!taskQueue.IsCompleted)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        public bool TryAddAction(Func<Task> action)
        {
            lastUsedAt = DateTime.UtcNow;
            return taskQueue.TryAdd(action);
        }

        public async Task StartProcess()
        {
            while (true)
            {
                if (taskQueue.TryTake(out var action))
                {
                    try
                    {
                        await action();
                    }
                    catch (Exception e)
                    {
                        //todo: log here
                    }
                }
                else
                {
                    await Task.Delay(10);
                }
            }
        }

        public bool IsExpired => DateTime.UtcNow - lastUsedAt > timeBudget;
    }
}