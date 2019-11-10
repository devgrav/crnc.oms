using System;
using Crnc.Oms.Domain.SeedWork;

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
        public FullName FullName { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public Email Email { get; set; }

        /// <summary>
        /// Phone
        /// </summary>
        public Phone Phone { get; set; }

        public Customer(Guid id, FullName fullName, Email email, Phone phone)
        {
            Id = id;
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Phone = phone ?? throw new ArgumentNullException(nameof(phone));
        }
    }
}
