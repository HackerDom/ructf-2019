using System;
using System.Threading.Tasks;
using NotificationsApi.Storage;

namespace NotificationsAPI
{
	internal class ExpirationDaemon
	{
		private readonly SourceStorage sourceStorage;
		private readonly TimeSpan period;

		public ExpirationDaemon(SourceStorage sourceStorage, TimeSpan period)
		{
			this.sourceStorage = sourceStorage;
			this.period = period;
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

				await Task.Delay(period);
			}
		}
	}
}