using System;
using System.Threading.Tasks;
using NotificationsApi.Storage;

namespace NotificationsAPI
{
	internal class ExpirationDaemon
	{
		private readonly TimeSpan delayTime = TimeSpan.FromMinutes(1);

		private readonly SourceStorage sourceStorage;

		public ExpirationDaemon(SourceStorage sourceStorage)
		{
			this.sourceStorage = sourceStorage;
			Run();
		}

		private async Task Run()
		{
			while(true)
			{
				foreach(var source in sourceStorage.GetAll())
				{
					while(source.Messages.TryPeek(out var res) && res.ExpireAt <= DateTime.UtcNow)
					{
						source.Messages.TryDequeue(out _);
					}
				}

				await Task.Delay(delayTime);
			}
		}
	}
}