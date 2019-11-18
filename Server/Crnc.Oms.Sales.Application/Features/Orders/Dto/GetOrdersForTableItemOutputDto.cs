using System;
using Crnc.Oms.Sales.Domain.Aggregates.Orders;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto
{
    public class GetOrdersForTableItemOutputDto
    {
        public Guid Id { get; set; }

        public string Number { get; set; }

        public string CreatedDate { get; set; }

        public string JobType { get; set; }

        public JobType JobTypeEnum { get; set; }

        public string JobDescription { get; set; }

        public string DateSentToCustomer { get; set; }
        
        public string Customer { get; set; }

        public string CustomerSignOffType { get; set; }

        public SignoffType? CustomerSignOffTypeEnum { get; set; }

        public string Status { get; set; }
    }
}