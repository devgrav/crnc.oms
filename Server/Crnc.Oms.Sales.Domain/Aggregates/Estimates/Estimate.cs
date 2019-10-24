using System;
using Crnc.Oms.Domain.SeedWork;
using Crnc.Oms.Sales.Domain.Aggregates.Customers;

namespace Crnc.Oms.Sales.Domain.Aggregates.Estimates
{
    /// <summary>
    /// Estimate for job
    /// </summary>
    public class Estimate
        : DomainEntity, IAggregateRoot
    {
        /// <summary>
        /// Number of estimate, tempalte E-value of id, may be put in manually
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
        /// Current status of estimate
        /// </summary>
        public EstimateStatus Status { get; set; }

        /// <summary>
        /// Source of material
        /// </summary>
        public MaterialSource MaterialSource { get; set; }

        /// <summary>
        /// Type of signoff
        /// </summary>
        public SignoffType SignoffType { get; set; }

        /// <summary>
        /// Customer of estimate
        /// </summary>
        public Customer Customer { get; set; }
    }
}
