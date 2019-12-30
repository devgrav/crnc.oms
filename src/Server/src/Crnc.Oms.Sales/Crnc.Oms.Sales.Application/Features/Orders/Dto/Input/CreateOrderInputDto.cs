using System;
using System.ComponentModel.DataAnnotations;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.WebApi.Validation;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto.Input
{
    public class CreateOrderInputDto
        : IUseCaseCommand<CreateOrderOutputDto>
    {
        public JobType JobType { get; set; }
        
        [Required]
        public string JobDescription { get; set; }

        [Required]
        public string CustomerContactPersonFirstName { get; set; }
        
        public string CustomerContactPersonMiddleName { get; set; }
        
        [Required]
        public string CustomerContactPersonLastName { get; set; }

        [Required]
        public string CustomerTitle { get; set; }
        
        [NameAbbreviationValueObject]
        public string CustomerAbbreviation { get; set; }
        
        [EmailValueObject]
        public string CustomerContactPersonEmail { get; set; }
        
        [PhoneValueObject]
        public string CustomerContactPersonPhone { get; set; }
    }
}