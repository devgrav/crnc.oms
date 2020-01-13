using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Security.Domain.Validation
{
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, 
        AllowMultiple = false)]
    public class RequiredNotEmptyAttribute : ValidationAttribute
    {
        public const string DefaultErrorMessage = "The {0} field is required.";
        public RequiredNotEmptyAttribute() : base(DefaultErrorMessage) { }

        public override bool IsValid(object value)
        {
            //NotEmpty doesn't necessarily mean required
            if (value is null)
            {
                return true;
            }

            switch (value)
            {
                case Guid guid:
                    return guid != Guid.Empty;
                default:
                    return true;
            }
        }
    }
}