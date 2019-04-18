using System.Net;
using NotificationsApi.Requests;
using System.Threading.Tasks;

namespace NotificationsApi.Handlers
{
    internal interface INotificationApiHandler
    {
        Task HandleAsync(NotificationApiRequest request); 
    }
}
