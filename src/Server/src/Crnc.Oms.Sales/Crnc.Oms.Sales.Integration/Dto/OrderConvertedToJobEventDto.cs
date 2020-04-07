using System;
using Crnc.Oms.Messaging.Contract.Events;

namespace Crnc.Oms.Sales.Integration.Dto
{
    public class OrderConvertedToJobEventDto
        : OrderConvertedToJobEvent
    {
        public string JobType { get; set; }
        public string JobDescription { get; set; }
        public string MaterialSource { get; set; }
        public string ManagerFullName { get; set; }
        public string ManagerLogin { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
    }
}