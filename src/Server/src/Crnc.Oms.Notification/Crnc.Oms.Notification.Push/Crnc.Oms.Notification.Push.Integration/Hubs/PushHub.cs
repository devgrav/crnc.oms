using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Push.Integration.Clients;
using Crnc.Oms.Notification.Push.Integration.Dto;
using Microsoft.AspNetCore.SignalR;

namespace Crnc.Oms.Notification.Push.Integration.Hubs
{
    public class PushHub
        : Hub<IPushNotificationClient>
    {

    }
}