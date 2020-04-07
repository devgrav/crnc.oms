using System;

namespace Crnc.Oms.Messaging.Contract.Events
{
    public interface OrderConvertedToJobEvent
    {
        string JobType { get; set; }
        
        string JobDescription { get; set; }

        string MaterialSource { get; set; }
        
        string ManagerFullName { get; set; }
        
        string ManagerLogin { get; set; }

        Guid OrderId { get; set; }

        string OrderNumber { get; set; }
    }
}