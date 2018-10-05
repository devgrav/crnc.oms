using Crnc.Oms.Domain.Aggregates.Customers;
using Crnc.Oms.Domain.Aggregates.Estimates;
using Crnc.Oms.Domain.Aggregates.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crnc.Oms.DataAccess.Data;

namespace Crnc.Oms.DataAccess
{
    public class FakeDataContext
    {
        public ICollection<User> Users = DataFactory.GetUsers(); 

        public ICollection<Role> Roles = DataFactory.GetRoles();

        public void SaveChanges()
        {
            return;
        }
    }
}
