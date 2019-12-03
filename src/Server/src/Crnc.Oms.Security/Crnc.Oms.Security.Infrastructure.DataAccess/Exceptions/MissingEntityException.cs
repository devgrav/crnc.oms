using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crnc.Oms.Security.Infrastructure.DataAccess.Exceptions
{
    /// <summary>
    /// Exception for missing entities
    /// </summary>
    public class MissingEntityException
        :ApplicationException
    {
        public MissingEntityException(string message = "Entity is not found")
            :base(message)
        {

        }
    }
}
