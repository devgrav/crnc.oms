using System;

namespace Crnc.Oms.Messaging.Contract.Events
{
    public interface JobCreatedForOrderEvent
    { 
        Guid JobId { get; set; }

        string JobNumber { get; set; }
        
        Guid OrderId { get; set; }
    }
}