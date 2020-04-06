using System;

namespace Crnc.Oms.Messaging.Contract.Commands
{
    public interface OrderConvertedToJobEvent
    {
        Guid UserId { get; set; }
        
        string Message { get; set; }
    }
}