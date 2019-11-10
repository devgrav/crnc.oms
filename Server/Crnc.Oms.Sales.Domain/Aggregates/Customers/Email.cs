using System;
using System.Text.RegularExpressions;
using Crnc.Oms.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Customers
{
    public class Email
        : IValueObject
    {
        public string Value { get; private set; }

        
        public Email(string email)
        {
            if(string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));

            if(!IsEmailValid(email))
                throw new DomainException("Email is not valid");
            
            Value = email;
        }

        protected Email()
        {
            
        }

        public static bool IsEmailValid(string email) =>
            new Regex(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$").IsMatch(email);
    }
}