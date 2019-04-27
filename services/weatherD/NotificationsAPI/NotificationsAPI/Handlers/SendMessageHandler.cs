using System;
using System.Net;
using System.Threading.Tasks;
using NotificationsApi.Documens;
using NotificationsApi.Requests;
using NotificationsApi.Storage;

namespace NotificationsApi.Handlers
{
    internal class SendMessageHandler : INotificationApiHandler
    {
	    private readonly MessageSender messageSender;
	    private readonly MongoDbClient mongoClient;
	    private readonly SourceStorage sourceStorage;
	    private readonly Authorizer authorizer;
	    private readonly TimeSpan ttl;


	    public SendMessageHandler(MessageSender messageSender,
			MongoDbClient mongoClient, 
			SourceStorage sourceStorage,
			Authorizer authorizer, 
			TimeSpan ttl)
	    {
		    this.messageSender = messageSender;
		    this.mongoClient = mongoClient;
		    this.sourceStorage = sourceStorage;
		    this.authorizer = authorizer;
		    this.ttl = ttl;
	    }
		public async Task HandleAsync(NotificationApiRequest request)
		{
			if(!authorizer.CanPush(request.source, request.password))
			{
				request.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return;
			}

			request.HttpContext.Response.StatusCode = (int)HttpStatusCode.Accepted;
			Task.Run(() =>
			{
				if(sourceStorage.TryGetInfo(request.source, out var info))
				{
					var message = new Message(request.Base64Message, DateTime.UtcNow + ttl);
					//await mongoClient.InsertMessage(request.SourceName, message);
					info.AddMessage(message);
					messageSender.Send(request.Base64Message, info);
				}
			});
		}
    }
}
