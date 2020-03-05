using System;

namespace Crnc.Oms.Messaging.Contract.Commands
{
    public interface SendEmailNotificationToUserCommand
    {
        Guid MessageId { get; set; }
        
        string SenderEmail { get; set; }

        string ReceiverEmail { get; set; }

        string Message { get; set; }
    }
}