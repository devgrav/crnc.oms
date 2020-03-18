using System;
using System.ComponentModel.DataAnnotations;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Application.Validation;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto.Input
{
    ///<summary>
    /// Edit order data
    /// </summary>
    /// <example>
    ///{
    ///    "id": "5c5c6017-1b1f-4a46-b423-455ad4f273fe",
    ///    "jobType": 2,
    ///    "jobDescription": "Some job",
    ///    "status": 2,
    ///    "customerTitle": "New customer",
    ///    "customerAbbreviation": "NC",
    ///    "customerContactPersonFirstName": "John",
    ///    "customerContactPersonLastName": "Smith",
    ///    "customerContactPersonEmail": "smith@nc.ru",
    ///    "customerContactPersonPhone": "+79151231212"
    ///}
    /// </example>
    public class EditOrderInputDto
        : IUseCaseCommand<EmptyOutputDto>
    {
        public Guid Id { get; set; }
        
        [EnumRequired(ErrorMessage = "Job type is required")]
        public JobType JobType { get; set; }
        
        [Required(ErrorMessage = "Job description is required")]
        public string JobDescription { get; set; }
        
        [EnumRequired]
        public OrderStatusEnum Status { get; set; }
        
        public MaterialSource? MaterialSource { get; set; }
        
        public SignoffType? SignOffType { get; set; }

        [Required(ErrorMessage = "Customer Title is required")]
        public string CustomerTitle { get; set; }
        
        [NameAbbreviationValueObject(ErrorMessage = "Customer Abbreviation is not valid", EmptyErrorMessage = "Customer Name Abbreviation is required")]
        public string CustomerAbbreviation { get; set; }
        
        [Required(ErrorMessage = "Contact Person First name is required")]
        public string CustomerContactPersonFirstName { get; set; }

        [Required(ErrorMessage = "Contact Person Last name is required")]
        public string CustomerContactPersonLastName { get; set; }
        
        [EmailValueObject(ErrorMessage = "Contact Person Email is not valid", EmptyErrorMessage = "Contact Person Email is required")]
        public string CustomerContactPersonEmail { get; set; }
        
        [PhoneValueObject(ErrorMessage = "Contact Person Phone is not valid", EmptyErrorMessage = "Contact Person Phone is required")]
        public string CustomerContactPersonPhone { get; set; }
    }
}