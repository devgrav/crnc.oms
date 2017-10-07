using Crnc.Oms.Domain.Aggregates.Customers;
using Crnc.Oms.Domain.Aggregates.Estimates;
using Crnc.Oms.Domain.Aggregates.Users;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crnc.Oms.DataAccess
{
    /// <summary>
    /// Data context of Entity Framework
    /// </summary>
    public class DataContext
        :DbContext
    {
        public DataContext(string connectionString)
            : base(connectionString)
        {

        }

        public DbSet<Estimate> Estimates { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Role> Roles { get; set; }

        public  DbSet<User> Users { get; set; }
    }
}
