using System.Collections.Concurrent;

namespace NotificationsApi.Storage
{
    internal class Authorizer
    {
        private readonly ConcurrentDictionary<string, (string token, string password)> storage;

	    private readonly ConcurrentDictionary<string, string> publicSources;
        public Authorizer()
        {
            storage = new ConcurrentDictionary<string, (string token, string password)>();
			publicSources = new ConcurrentDictionary<string, string>();
        }

        public void RegisterPrivate(string src, string token, string password)
        {
			storage.AddOrUpdate(src, (token, password), (a, b) => b);
        }

	    public void RegisterPublic(string src,string password)
	    {
		    publicSources.AddOrUpdate(src, password, (a, b) => b);
	    }

		public bool CanSubscribe(string src, string token="")
		{
			if(publicSources.ContainsKey(src))
				return true;

			return storage.TryGetValue(src, out var v) && v.token == token;
	    }

	    public bool CanPush(string src, string password)
	    {
		    return storage.TryGetValue(src, out var v) && v.password == password || publicSources.TryGetValue(src, out var v2) && v2 == password;
	    }
	}
}
