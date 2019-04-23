using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NotificationsAPI.SSE
{
	public class SSEClient
	{
		public async Task SendMessageAsync(HttpContext httpContext, byte[] message)
		{
			try
			{
				await httpContext.Response.Body.WriteAsync(message);
				await httpContext.Response.WriteAsync("\n\n");
				await httpContext.Response.Body.FlushAsync();
			}
			catch(Exception e)
			{

			}
		}
	}
}