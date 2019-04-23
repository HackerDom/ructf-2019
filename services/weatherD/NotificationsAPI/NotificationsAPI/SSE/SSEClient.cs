using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NotificationsAPI.SSE
{
	public class SseClient
	{
		public async Task SendMessageAsync(HttpContext httpContext, string message)
		{
			try
			{
				await httpContext.Response.WriteAsync(message);
				await httpContext.Response.WriteAsync("\n\n");
				await httpContext.Response.Body.FlushAsync();
			}
			catch(Exception e)
			{
			}
		}
	}
}