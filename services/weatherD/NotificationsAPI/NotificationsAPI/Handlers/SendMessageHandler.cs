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
	    private readonly TimeSpan TTL = TimeSpan.FromMinutes(60);


		public SendMessageHandler(MessageSender messageSender, MongoDbClient mongoClient, SourceStorage sourceStorage, Authorizer authorizer)
	    {
		    this.messageSender = messageSender;
		    this.mongoClient = mongoClient;
		    this.sourceStorage = sourceStorage;
		    this.authorizer = authorizer;
	    }
		public async Task HandleAsync(NotificationApiRequest request)
		{
			if(!authorizer.CanPush(request.SourceName, request.Password))
			{
				request.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return;
			}

			if(sourceStorage.TryGetInfo(request.SourceName, out var info))
			{
				var message = new Message(request.Message, DateTime.UtcNow + TTL);
				//await mongoClient.InsertMessage(request.SourceName, message);
				info.AddMessage(message);
				messageSender.Send(request.Message, info);
			}

			request.HttpContext.Response.StatusCode = (int) HttpStatusCode.Accepted;
		}
    }
}
