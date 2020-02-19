using System;
using Crnc.Oms.Notification.Contract;

namespace Crnc.Oms.Sales.Integration.Dto
{
    public class NotificationUserDto
        : NotificationUser
    {
        public Guid UserId { get; set; }
        
        public string Message { get; set; }
    }
}