using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crnc.Oms.Domain.Aggregates.Estimates
{
    /// <summary>
    /// Types of job for estimate
    /// </summary>
    public enum JobType
    {
        New = 1,

        Repair = 2,

        Service = 3,

        Other = 4
    }
}
