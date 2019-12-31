using System;
using System.ComponentModel.DataAnnotations;
using Crnc.Oms.Sales.Domain.Aggregates.Order;

namespace Crnc.Oms.Sales.Application.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
        public sealed class EnumRequiredAttribute : ValidationAttribute
        {
            public string ErrorMessage { get; set; }
            
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var errorMessage = !string.IsNullOrWhiteSpace(ErrorMessage) 
                    ? ErrorMessage 
                    : $"{validationContext.MemberName} is required";

                if (value == null)                   
                    return new ValidationResult(errorMessage); 

                int validatedValue = (int)value;
            
                if(validatedValue == 0)
                    return new ValidationResult(errorMessage);
                    
            
                return ValidationResult.Success;
            }
        }
    }
