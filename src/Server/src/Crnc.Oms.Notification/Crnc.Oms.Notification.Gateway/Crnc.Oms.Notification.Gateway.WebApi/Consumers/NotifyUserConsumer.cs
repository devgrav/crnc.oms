using System;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Contract;
using Crnc.Oms.Notification.Gateway.Application.Dto;
using Crnc.Oms.Notification.Gateway.Application.Services.Abstractions;
using MassTransit;

namespace Crnc.Oms.Notification.Gateway.WebApi.Consumers
{
    public class NotifyUserConsumer :
            IConsumer<NotificationUser>
        {
            private readonly INotificationService _notificationService;

            public NotifyUserConsumer(INotificationService notificationService)
            {
                _notificationService = notificationService;
            }

            public async Task Consume(ConsumeContext<NotificationUser> context)
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