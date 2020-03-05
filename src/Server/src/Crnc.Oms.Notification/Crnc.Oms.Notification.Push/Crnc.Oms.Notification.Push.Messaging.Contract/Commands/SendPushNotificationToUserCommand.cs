using System;

namespace Crnc.Oms.Messaging.Contract.Commands
{
    public interface SendPushNotificationToUserCommand
    {
        Guid MessageId { get; set; }
        
        Guid ReceiverUserId { get; set; }
        
        string Message { get; set; }
    }
}