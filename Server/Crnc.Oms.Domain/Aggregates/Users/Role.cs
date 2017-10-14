using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crnc.Oms.Domain.Aggregates.Users
{
    /// <summary>
    /// Role of user
    /// </summary>
    public class Role
        : DomainEntity
    {
        /// <summary>
        /// Title of role
        /// </summary>
        [MaxLength(50)]
        [Required]
        public string Title { get; set; }

        public Role()
        {

        }

        public Role(string title)
        {
            Title = title;
        }
    }
}
