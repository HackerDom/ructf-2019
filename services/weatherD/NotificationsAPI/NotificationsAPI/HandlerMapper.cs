using System.Collections.Concurrent;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using NotificationsApi.Handlers;

namespace NotificationsApi
{
    internal class HandlerMapper
    {
	    private readonly ConcurrentDictionary<(string, HttpMethod), INotificationApiHandler> mapping = new ConcurrentDictionary<(string, HttpMethod), INotificationApiHandler>();

	    public void Add(string methodPath, HttpMethod method, INotificationApiHandler handler)
	    {
		    mapping.TryAdd((methodPath, method), handler);
	    }

	    public INotificationApiHandler Get(string methodPath, HttpMethod method)
	    {
		    mapping.TryGetValue((methodPath, method), out var result);
		    return result;
	    }
    }
}
