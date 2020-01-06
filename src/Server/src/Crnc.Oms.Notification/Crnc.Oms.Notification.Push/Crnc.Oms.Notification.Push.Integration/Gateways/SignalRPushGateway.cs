using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Push.Integration.Clients;
using Crnc.Oms.Notification.Push.Integration.Dto;
using Crnc.Oms.Notification.Push.Integration.Gateways.Abstractions;
using Crnc.Oms.Notification.Push.Integration.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Crnc.Oms.Notification.Push.Integration.Gateways
{
    public class SignalRPushGateway
        : IPushGateway
    {
        private readonly IHubContext<PushHub, IPushNotificationClient> _pushHubContext;

        public SignalRPushGateway(IHubContext<PushHub, IPushNotificationClient> pushHubContext)
        {
            _pushHubContext = pushHubContext;
        }
        
        public async Task<PushMessageOutputDto> SendPushAsync(PushMessageInputDto dto, CancellationToken cancellationToken = default)
        {
            await _pushHubContext.Clients.User(dto.ReceiverUserId.ToString()).ReceivePushMessageAsync(dto.ReceiverUserId.ToString(), dto.Message);

            return new PushMessageOutputDto()
            {
                MessageId = dto.MessageId
            };
        }
    }
}