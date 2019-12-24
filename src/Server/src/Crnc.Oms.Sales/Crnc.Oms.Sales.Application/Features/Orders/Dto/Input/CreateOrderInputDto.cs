using System;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.WebApi.Validation;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto.Input
{
    public class CreateOrderInputDto
        : IUseCaseCommand<CreateOrderOutputDto>
    {
        public JobType JobType { get; set; }
        public string JobDescription { get; set; }

        public string FirstName { get; set; }
        
        public string MiddleName { get; set; }
        
        public string LastName { get; set; }
        
        [NameAbbreviationValueObject]
        public string Abbreviation { get; set; }
        
        [EmailValueObject]
        public string Email { get; set; }
        
        [PhoneValueObject]
        public string Phone { get; set; }
    }
}