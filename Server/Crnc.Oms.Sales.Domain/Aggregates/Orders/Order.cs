using System;
using Crnc.Oms.Domain.SeedWork;
using Crnc.Oms.Sales.Domain.Aggregates.Customers;

namespace Crnc.Oms.Sales.Domain.Aggregates.Orders
{
    /// <summary>
    /// Order for job
    /// </summary>
    public class Order
        : DomainEntity, IAggregateRoot
    {
        /// <summary>
        /// Number of order, tempalte E-value of id, may be put in manually
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Date of created
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Date sent to customer
        /// </summary>
        public DateTime? DateSentToCustomer { get; set; }

        /// <summary>
        /// Job type
        /// </summary>
        public JobType JobType { get; set; }

        /// <summary>
        /// Comments
        /// </summary>
        public string JobDescription { get; set; }

        /// <summary>
        /// Current status of order
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Source of material
        /// </summary>
        public MaterialSource MaterialSource { get; set; }

        /// <summary>
        /// Type of signoff
        /// </summary>
        public SignoffType SignOffType { get; set; }
        
        /// <summary>
        /// Customer Id
        /// </summary>
        public Guid CustomerId { get; set; }
        
        /// <summary>
        /// Customer of order
        /// </summary>
        public virtual Customer Customer { get; set; }
    }
}
