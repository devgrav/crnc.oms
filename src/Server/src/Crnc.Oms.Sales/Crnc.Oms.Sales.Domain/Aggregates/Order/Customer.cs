using System;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    /// <summary>
    /// Customer
    /// </summary>
    public class Customer
        : IValueObject
    {
        /// <summary>
        /// Title of customer
        /// </summary>
        public Title Title { get; private set; }

        /// <summary>
        /// Contact person of customer
        /// </summary>
        public ContactPerson ContactPerson { get; private set; }

        public Customer(Title title, ContactPerson contactPerson)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            ContactPerson = contactPerson ?? throw new ArgumentNullException(nameof(contactPerson));
        }

        protected Customer()
        {
            
        }
    }
}
