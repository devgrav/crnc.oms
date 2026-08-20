using System;
using System.Threading.Tasks;
using Crnc.Oms.Messaging.Contract.Commands;
using Crnc.Oms.Notification.Email.Application.Dto;
using Crnc.Oms.Notification.Email.Application.Services.Abstractions;
using MassTransit;

namespace Crnc.Oms.Notification.Email.WebApi.Consumers
{
    public class SendEmailNotificationToReceiverConsumer:
            IConsumer<SendEmailNotificationToReceiverCommand>
        {
            private readonly IEmailNotificationService _notificationService;

            public SendEmailNotificationToReceiverConsumer(IEmailNotificationService notificationService)
            {
                _notificationService = notificationService;
            }

            public async Task Consume(ConsumeContext<SendEmailNotificationToReceiverCommand> context)
            {
                var notification = context.Message;
                
                if(notification == null)
                    throw new ArgumentNullException(nameof(notification));

                await _notificationService.SendAsync(new SendEmailMessageInputDto()
                {
                    MessageId = notification.MessageId,
                    SenderEmail = notification.SenderEmail,
                    ReceiverEmail = notification.ReceiverEmail,
                    Message = notification.Message
                });
            }
        }
    
}