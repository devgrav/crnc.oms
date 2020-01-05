using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Push.Integration.Dto;

namespace Crnc.Oms.Notification.Push.Integration.Clients
{
    public interface IPushNotificationClient
    {
        Task ReceivePushMessageAsync(string userId, string message);
    }
}