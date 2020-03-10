using System;

namespace Crnc.Oms.Messaging.Contract.Commands
{
    public interface SendNotificationToUserCommand
    {
        Guid UserId { get; set; }
        
        string Message { get; set; }
    }
}