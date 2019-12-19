using System;
using Crnc.Oms.Sales.Domain.Aggregates.Order;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto
{
    public class EditOrderInputDto
        : IUseCaseCommand
    {
        public Guid Id { get; set; }

        public JobType JobType { get; set; }
        
        public string JobDescription { get; set; }
        
        public OrderStatus Status { get; set; }
        
        public MaterialSource? MaterialSource { get; set; }
        
        public SignoffType? SignOffType { get; set; }

        public string FirstName { get; set; }
        
        public string MiddleName { get; set; }
        
        public string LastName { get; set; }
        
        public string Abbreviation { get; set; }
        
        public string Email { get; set; }
        
        public string Phone { get; set; }
    }
}