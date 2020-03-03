using System;
using System.Threading.Tasks;
using Crnc.Oms.Messaging.Contract.Commands;
using Crnc.Oms.Notification.Gateway.Application.Dto;
using Crnc.Oms.Notification.Gateway.Application.Services.Abstractions;
using MassTransit;

namespace Crnc.Oms.Notification.Gateway.WebApi.Consumers
{
    public class SendNotificationToUser:
            IConsumer<SendNotificationToUserCommand>
        {
            private readonly INotificationService _notificationService;

            public SendNotificationToUser(INotificationService notificationService)
            {
                _notificationService = notificationService;
            }

            public async Task Consume(ConsumeContext<SendNotificationToUserCommand> context)
            {
                var notification = context.Message;
                
                if(notification == null)
                    throw new ArgumentNullException(nameof(notification));

                await _notificationService.SendNotificationToUserAsync(new SendToNotificationUserInputDto()
                {
                    Message = notification.Message,
                    UserId = notification.UserId
                });
            }
        }
    
}