using System;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    public class FullName
        : IValueObject
    {
        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public string Value => $"{FirstName} {LastName}";

        public FullName(string firstName, string lastName)
        {
            if(string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentNullException(nameof(firstName));;
            
            if(string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentNullException(nameof(lastName));

            FirstName = firstName;
            LastName = lastName;
        }

        protected FullName()
        {
            
        }

        public override string ToString()
        {
            return Value;
        }
    }
}