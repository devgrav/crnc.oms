using System;
using Crnc.Oms.Sales.Domain.Aggregates.Orders;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto
{
    public class GetNewOrderOutputDto
    {
        public OrderStatus StatusEnum { get; set; }

        public string Status { get; set; }

        public string DateCreated { get; set; }

        public string JobType { get; set; }

        public JobType? JobTypeEnum { get; set; }

        public string JobDescription { get; set; }
        public GetNewOrderCustomerOutputDto Customer { get; set; }

    }

    public class GetNewOrderCustomerOutputDto
    {
        public string FullName { get; set; }
        
        public string Abbreviation { get; set; }
        
        public string Email { get; set; }
        
        public string Phone { get; set; }
    }
}