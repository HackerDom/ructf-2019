using System;
using System.Text;
using NotificationsApi.Requests;

namespace NotificationsAPI
{
	internal class TokensGenerator
	{
		public static string Generate(NotificationApiRequest request)
		{
			var timeBytes = BitConverter.GetBytes(request.timestamp);
			var part1 = BitConverter.ToInt32(new[] { timeBytes[0], timeBytes[1], timeBytes[2], timeBytes[3] });
			var part2 = BitConverter.ToInt16(new[] { timeBytes[4], timeBytes[5] });
			var part3 = BitConverter.ToInt16(new[] { timeBytes[6], timeBytes[7] });
			var srcBytes = Encoding.UTF8.GetBytes(request.source);
			var bytes = new byte[8];
			for(var i = 0; i < Math.Min(srcBytes.Length, 8); i++)
				bytes[i] = srcBytes[i];
			var guid = new Guid(part1, part2, part3, bytes);
			return guid.ToString();
		}
	}
}