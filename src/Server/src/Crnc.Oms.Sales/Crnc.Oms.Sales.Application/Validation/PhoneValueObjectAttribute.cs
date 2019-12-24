using System;
using System.ComponentModel.DataAnnotations;
using Crnc.Oms.Sales.Domain.Aggregates.Order;

namespace Crnc.Oms.Sales.WebApi.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PhoneValueObjectAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)                   
                return ValidationResult.Success; 

            string phone = value as string;

            if (!Phone.IsValidPhone(phone))
                return new ValidationResult("Phone is not valid");

            return ValidationResult.Success;
        }
    }
}