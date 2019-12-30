using System;
using System.ComponentModel.DataAnnotations;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.WebApi.Validation;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto.Input
{
    public class EditOrderInputDto
        : IUseCaseCommand<EmptyOutputDto>
    {
        public Guid Id { get; set; }

        public JobType JobType { get; set; }
        
        public string JobDescription { get; set; }
        
        public OrderStatus Status { get; set; }
        
        public MaterialSource? MaterialSource { get; set; }
        
        public SignoffType? SignOffType { get; set; }

        [Required]
        public string CustomerFirstName { get; set; }
        
        public string CustomerMiddleName { get; set; }
        
        [Required]
        public string CustomerLastName { get; set; }

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