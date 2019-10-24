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
        public string FullName { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Phone
        /// </summary>
        public string Phone { get; set; }
    }
}
