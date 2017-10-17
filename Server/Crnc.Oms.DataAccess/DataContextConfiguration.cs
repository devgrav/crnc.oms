using Crnc.Oms.DataAccess.DbInitialize;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crnc.Oms.DataAccess
{
    /// <summary>
    /// Data context configuration
    /// </summary>
    class DataContextConfiguration
        : DbConfiguration
    {
        public DataContextConfiguration()
        {
            SetDatabaseInitializer(new DataContextDbInitializer());
            SetProviderServices(SqlProviderServices.ProviderInvariantName, SqlProviderServices.Instance);
        }
    }
}
