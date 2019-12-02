using System;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Customers
{
    /// <summary>
    /// Customer
    /// </summary>
    public class Customer
        : DomainEntity, IAggregateRoot
    {
        /// <summary>
        /// Full name
        /// </summary>
        public FullName FullName { get; private set; }

        /// <summary>
        /// Abbreviation of name
        /// </summary>
        public NameAbbreviation Abbreviation { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public Email Email { get;private set; }

        /// <summary>
        /// Phone
        /// </summary>
        public Phone Phone { get; private set; }

        public Customer(Guid id, FullName fullName, Email email, Phone phone)
            : base(id)
        {
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Phone = phone ?? throw new ArgumentNullException(nameof(phone));
        }

        protected Customer()
        {
            
        }
    }
}
