using System;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    public class NameAbbreviation
    {
        public string Value { get; set; }
        
        public NameAbbreviation(string abbreviation)
        {
            if(string.IsNullOrWhiteSpace(abbreviation))
                throw new ArgumentNullException(nameof(abbreviation));

            if (abbreviation.Length != 2)
                throw new Exception("Name abbreviation must be 2 symbols");

            Value = abbreviation;
        }

        protected NameAbbreviation()
        {
            
        }
    }
}