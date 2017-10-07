using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crnc.Oms.Domain.Aggregates.Estimates
{
    /// <summary>
    /// Source of material
    /// </summary>
    public enum MaterialSource
    {
        ToBeOrdered =1,

        IncludedByCustomer = 2,

        UserStock = 3
    }
}
