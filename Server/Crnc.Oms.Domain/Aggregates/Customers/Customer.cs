using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crnc.Oms.Domain.Aggregates.Customers
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
        [MaxLength(255)]
        [Required]
        public string FullName { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [MaxLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// Phone
        /// </summary>
        [MaxLength(50)]
        public string Phone { get; set; }
    }
}
