using System;

namespace Crnc.Oms.Notification.Contract
{
    public interface NotificationUser
    {
        Guid UserId { get; set; }
        
        string Message { get; set; }
    }
}