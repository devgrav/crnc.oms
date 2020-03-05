using System;
using System.Threading.Tasks;
using Crnc.Oms.Messaging.Contract.Commands;
using Crnc.Oms.Notification.Push.Application.Dto;
using Crnc.Oms.Notification.Push.Application.Services.Abstractions;
using MassTransit;

namespace Crnc.Oms.Notification.Push.WebApi.Consumers
{
    public class SendPushNotificationToUserReceiverConsumer:
            IConsumer<SendPushNotificationToReceiverCommand>
        {
            private readonly IPushNotificationService _notificationService;

            public SendPushNotificationToUserReceiverConsumer(IPushNotificationService notificationService)
            {
                _notificationService = notificationService;
            }

            public async Task Consume(ConsumeContext<SendPushNotificationToReceiverCommand> context)
            {
                var notification = context.Message;
                
                if(notification == null)
                    throw new ArgumentNullException(nameof(notification));

                await _notificationService.SendAsync(new SendPushMessageInputDto()
                {
                    Message = notification.Message,
                    MessageId = notification.MessageId,
                    ReceiverUserId = notification.ReceiverUserId
                });
            }
        }
}