using System.Net;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Http;
using NotificationsApi.Requests;
using NotificationsApi.Storage;
using NotificationsAPI;

namespace NotificationsApi.Handlers
{
    internal class AddSourceInfoHandler : INotificationApiHandler
    {
	    private readonly MongoDbClient mongoDbClient;
	    private readonly Authorizer authorizer;
	    private readonly SourceStorage sourceStorage;
		private readonly ILog log;

	    public AddSourceInfoHandler(MongoDbClient mongoDbClient, Authorizer authorizer, SourceStorage sourceStorage, ILog log)
	    {
		    this.mongoDbClient = mongoDbClient;
		    this.authorizer = authorizer;
		    this.log = log;
		    this.sourceStorage = sourceStorage;
	    }

        public async Task HandleAsync(NotificationApiRequest request)
        {
	        var token = request.IsPublic ? null : TokensGenerator.Generate(request);

	    //    await mongoDbClient.InsertUser(request.SourceName, token, request.Password, request.IsPublic);

	        if(request.IsPublic)
		        authorizer.RegisterPublic(request.source, request.password);
	        else
		        authorizer.RegisterPrivate(request.source, token, request.password);

	        sourceStorage.Add(request.source);
	        request.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
			await request.HttpContext.Response.WriteAsync(token);
	        await request.HttpContext.Response.Body.FlushAsync();

        }
    }
}
