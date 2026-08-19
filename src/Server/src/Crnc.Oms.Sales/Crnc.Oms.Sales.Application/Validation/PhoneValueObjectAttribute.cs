using System;
using System.ComponentModel.DataAnnotations;
using Crnc.Oms.Sales.Domain.Aggregates.Order;

namespace Crnc.Oms.Sales.Application.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PhoneValueObjectAttribute : ValidationAttribute
    {
        public PhoneValueObjectAttribute()
            : base("Phone is not valid")
        {
            
        }
        
        public string EmptyErrorMessage { get; set; } = "Phone is required";
        
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)                   
                return new ValidationResult(EmptyErrorMessage); 

            string validatedValue = value as string;
            
            if(string.IsNullOrWhiteSpace(validatedValue))
                return new ValidationResult(EmptyErrorMessage);

            if (!Phone.IsValidPhone(validatedValue))
                return new ValidationResult(ErrorMessage);
            
            return ValidationResult.Success;
        }
    }
}