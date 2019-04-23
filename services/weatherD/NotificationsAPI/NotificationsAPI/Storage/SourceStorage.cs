using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NotificationsApi.Documens;

namespace NotificationsApi.Storage
{
	internal class SourceStorage
	{
		private readonly ConcurrentDictionary<string, SourceInfo> srcToSourceInfos = new ConcurrentDictionary<string, SourceInfo>();

		public bool TryGetInfo(string source, out SourceInfo sourceInfo)
		{
			return srcToSourceInfos.TryGetValue(source, out sourceInfo);
		}

		public void Add(string source)
		{
			srcToSourceInfos.TryAdd(source, new SourceInfo());
		}

		public IEnumerable<SourceInfo> GetAll() => srcToSourceInfos.Select(x => x.Value);
	}
}