using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Push.Integration.Clients;
using Crnc.Oms.Notification.Push.Integration.Dto;
using Crnc.Oms.Notification.Push.Integration.Gateways.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Crnc.Oms.Notification.Push.Integration.Gateways
{
    public class SignalRPushGateway
        : Hub<IPushNotificationClient>, IPushGateway
    {
        public async Task<PushMessageOutputDto> SendPushAsync(PushMessageInputDto dto, CancellationToken cancellationToken = default)
        {
            await Clients.User(dto.ReceiverUserId.ToString()).ReceivePushMessageAsync(dto.ReceiverUserId.ToString(), dto.Message);

            return new PushMessageOutputDto()
            {
                MessageId = dto.MessageId
            };
        }
    }
}