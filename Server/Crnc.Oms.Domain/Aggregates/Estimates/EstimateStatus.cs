using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crnc.Oms.Domain.Aggregates.Estimates
{
    /// <summary>
    /// Status of estimate
    /// </summary>
    public enum EstimateStatus
    {
        NotSent=1,

        NeedSignoff=2,

        Signed=3,

        ConvertedToJob=4,

        Closed=5   
    }
}
