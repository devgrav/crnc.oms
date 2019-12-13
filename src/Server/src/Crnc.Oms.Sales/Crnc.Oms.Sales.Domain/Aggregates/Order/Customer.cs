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
        /// Full name
        /// </summary>
        public FullName FullName { get; private set; }

        /// <summary>
        /// Abbreviation of name
        /// </summary>
        public NameAbbreviation Abbreviation { get; private set; }

        /// <summary>
        /// Email
        /// </summary>
        public Email Email { get;private set; }

        /// <summary>
        /// Phone
        /// </summary>
        public Phone Phone { get; private set; }

        public Customer(FullName fullName, NameAbbreviation abbreviation, Email email, Phone phone)
        {
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Phone = phone ?? throw new ArgumentNullException(nameof(phone));
            Abbreviation = abbreviation ?? throw new ArgumentNullException(nameof(abbreviation));
        }

        protected Customer()
        {
            
        }
    }
}
