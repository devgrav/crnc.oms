using System;

namespace Crnc.Oms.Sales.Integration.Dto
{
    public class NotificationUserDto
    {
        public Guid UserId { get; set; }
        
        public string Message { get; set; }
    }
}