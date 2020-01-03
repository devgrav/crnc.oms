using System;

namespace Crnc.Oms.Notification.Gateway.Integration.Exceptions
{
    public class MissingUserException
        : Exception
    {
        public MissingUserException(string message = "User have not found")
            : base(message)
        {
            
        }

        public MissingUserException(string message, Exception exception)
            : base(message, exception)
        {
            
        }
    }
}