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

            if (!IsNameAbbreviationValid(abbreviation))
                throw new Exception("Name abbreviation is not valid");

            Value = abbreviation;
        }

        public static bool IsNameAbbreviationValid(string abbreviation) => abbreviation.Length == 2;

        protected NameAbbreviation()
        {
            
        }
    }
}