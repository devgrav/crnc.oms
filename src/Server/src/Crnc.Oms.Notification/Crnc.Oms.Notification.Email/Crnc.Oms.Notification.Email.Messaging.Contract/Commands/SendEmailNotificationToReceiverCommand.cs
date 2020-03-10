using System;

namespace Crnc.Oms.Messaging.Contract.Commands
{
    public interface SendEmailNotificationToReceiverCommand
    {
        Guid MessageId { get; set; }
        
        string SenderEmail { get; set; }

        string ReceiverEmail { get; set; }

        string Message { get; set; }
    }
}