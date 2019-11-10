using System;
using System.Text.RegularExpressions;
using Crnc.Oms.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Customers
{
    public class Phone
        : IValueObject
    {
        public string Value { get; private set; }
        
        public Phone(string phone)
        {
            if(string.IsNullOrWhiteSpace(phone))
                throw new ArgumentNullException(nameof(phone));
            
            if(!IsValidPhone(phone))
                throw new DomainException("Phone is not valid");
            
            Value = phone;
        }

        protected Phone()
        {
            
        }

        public static bool IsValidPhone(string phone) =>
            new Regex(@"^(\+7|7|8)?[\s\-]?\(?[489][0-9]{2}\)?[\s\-]?[0-9]{3}[\s\-]?[0-9]{2}[\s\-]?[0-9]{2}$").IsMatch(phone);
    }
}