using System;
using Crnc.Oms.Messaging.Contract.Events;

namespace Crnc.Oms.Production.Integration.Dto
{
    public class JobCreatedForOrderDto
        : JobCreatedForOrderEvent
    {
        public Guid JobId { get; set; }
        public string JobNumber { get; set; }
        public Guid OrderId { get; set; }
    }
}