using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crnc.Oms.Security.Domain.SeedWork;

namespace Crnc.Oms.Security.Domain.Aggregates.Users
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
        public string Title { get; set; }

        public Role()
        {

        }

        public Role(string title)
        {
            Id = Guid.NewGuid();
            Title = title;
        }
    }
}
