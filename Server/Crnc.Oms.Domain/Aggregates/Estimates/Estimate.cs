using Crnc.Oms.Domain.Aggregates.Customers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crnc.Oms.Domain.Aggregates.Estimates
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
        [MaxLength(50)]
        [Required]
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
        [MaxLength(255)]
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
