using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NotificationsApi.Storage
{
    internal class Authorizer
    {
        private readonly ConcurrentDictionary<string, (string token, string password)> storage;
        public Authorizer(Dictionary<string, (string token, string password)> init)
        {
            storage = new ConcurrentDictionary<string, (string token, string password)>(init.ToList());
        }

        public void Register(string src, string token, string password)
        {
	        storage.AddOrUpdate(src, (token, password), (a, b) => b);
        }

	    public bool CanSubscribe(string src, string token)
	    {
		    return storage.TryGetValue(src, out var v) && v.token == token;
	    }

	    public bool CanPush(string src, string password)
	    {
		    return storage.TryGetValue(src, out var v) && v.password == password;
	    }
	}
}
