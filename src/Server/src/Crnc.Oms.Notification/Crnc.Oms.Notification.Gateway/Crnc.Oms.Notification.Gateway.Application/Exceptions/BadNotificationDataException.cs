using System;

namespace Crnc.Oms.Notification.Gateway.Application.Exceptions
{
    public class BadNotificationDataException
        : Exception
    {
        public BadNotificationDataException(string message = "Notification data is not valid")
            : base(message)
        {
            
        }
        
        public BadNotificationDataException(string message, Exception exception)
            : base(message, exception)
        {
            
        }
    }
}