using System;
using System.ComponentModel.DataAnnotations;
using Crnc.Oms.Sales.Domain.Aggregates.Order;

namespace Crnc.Oms.Sales.WebApi.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EmailValueObjectAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)                   
                return ValidationResult.Success; 

            string email = value as string;

            if (!Email.IsEmailValid(email))
                return new ValidationResult("Email is not valid");

            return ValidationResult.Success;
        }
    }
}