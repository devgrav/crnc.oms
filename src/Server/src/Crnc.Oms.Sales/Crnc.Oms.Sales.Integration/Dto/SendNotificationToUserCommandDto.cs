using System;
using Crnc.Oms.Messaging.Contract.Commands;

namespace Crnc.Oms.Sales.Integration.Dto
{
    public class SendNotificationToUserCommandDto
        : SendNotificationToUserCommand
    {
        public Guid UserId { get; set; }
        
        public string Message { get; set; }
    }
}