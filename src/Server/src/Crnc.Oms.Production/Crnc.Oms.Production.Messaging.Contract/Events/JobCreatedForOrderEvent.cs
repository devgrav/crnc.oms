using System;

namespace Crnc.Oms.Messaging.Contract.Events
{
    public interface JobCreatedEvent
    { 
        Guid JobId { get; set; }

        string JobNumber { get; set; }
    }
}