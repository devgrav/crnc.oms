using System;
using System.ComponentModel.DataAnnotations;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Application.Validation;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto.Input
{
    public class CreateOrderInputDto
        : IUseCaseCommand<CreateOrderOutputDto>
    {
        [EnumRequired(ErrorMessage = "Job type is required")]
        public JobType JobType { get; set; }
        
        [Required(ErrorMessage = "Job description is required")]
        public string JobDescription { get; set; }

        [Required(ErrorMessage = "Customer Title is required")]
        public string CustomerTitle { get; set; }
        
        [NameAbbreviationValueObject(ErrorMessage = "Customer Abbreviation is not valid", EmptyErrorMessage = "Customer Name Abbreviation is required")]
        public string CustomerAbbreviation { get; set; }
        
        [Required(ErrorMessage = "Contact Person First name is required")]
        public string CustomerContactPersonFirstName { get; set; }
        
        public string CustomerContactPersonMiddleName { get; set; }
        
        [Required(ErrorMessage = "Contact Person Last name is required")]
        public string CustomerContactPersonLastName { get; set; }
        
        [EmailValueObject(ErrorMessage = "Contact Person Email is not valid", EmptyErrorMessage = "Contact Person Email is required")]
        public string CustomerContactPersonEmail { get; set; }
        
        [PhoneValueObject(ErrorMessage = "Contact Person Phone is not valid", EmptyErrorMessage = "Contact Person Phone is required")]
        public string CustomerContactPersonPhone { get; set; }
    }
}