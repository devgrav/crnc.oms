using System.Collections.Generic;
using Crnc.Oms.DataAccess.Data;
using Crnc.Oms.Domain.Aggregates.Users;

namespace Crnc.Oms.DataAccess
{
    public class FakeDataContext
    {
        public List<User> Users = DataFactory.GetUsers(DataFactory.GetRoles());

        public List<Role> Roles = DataFactory.GetRoles();

        public void SaveChanges()
        {

        }
    }
}