using System;
using System.ComponentModel.DataAnnotations;
using Crnc.Oms.Sales.Domain.Aggregates.Order;

namespace Crnc.Oms.Sales.WebApi.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NameAbbreviationValueObjectAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)                   
                return ValidationResult.Success; 

            string abbreviation = value as string;

            if (!NameAbbreviation.IsNameAbbreviationValid(abbreviation))
                return new ValidationResult("Name abbreviation must be 2 symbols");

            return ValidationResult.Success;
        }
    }
}