using System;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    public class Title
        : IValueObject
    {
        public string Value { get; private set; }
        
        public NameAbbreviation NameAbbreviation { get; private set; }

        public Title(string title, NameAbbreviation nameAbbreviation)
        {
            if(string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title));

            Value = title;
            NameAbbreviation = nameAbbreviation;
        }

        protected Title()
        {
            
        }
    }
}