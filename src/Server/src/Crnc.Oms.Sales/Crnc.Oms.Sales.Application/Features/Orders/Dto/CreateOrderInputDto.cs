using System;
using Crnc.Oms.Sales.Domain.Aggregates.Order;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto
{
    public class CreateOrderInputDto
        : IUseCaseCommand
    {
        public JobType JobType { get; set; }
        public string JobDescription { get; set; }

        public string FirstName { get; set; }
        
        public string MiddleName { get; set; }
        
        public string LastName { get; set; }
        
        public string Abbreviation { get; set; }
        
        public string Email { get; set; }
        
        public string Phone { get; set; }
    }
}