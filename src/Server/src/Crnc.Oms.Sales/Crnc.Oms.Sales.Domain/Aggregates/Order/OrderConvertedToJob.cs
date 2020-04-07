using System;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    public class OrderConvertedToJob
        : DomainEvent
    {
        public JobInfo JobInfo { get; private set; }

        public MaterialSource MaterialSource { get; private set; }
        
        public Manager Manager { get; private set; }

        public Guid OrderId { get; private set; }

        public string OrderNumber { get; private set; }

        public OrderConvertedToJob(JobInfo jobInfo, MaterialSource materialSource, Manager manager, Guid orderId, string orderNumber)
        {
            JobInfo = jobInfo;
            MaterialSource = materialSource;
            Manager = manager;
            OrderId = orderId;
            OrderNumber = orderNumber;
        }
    }
}