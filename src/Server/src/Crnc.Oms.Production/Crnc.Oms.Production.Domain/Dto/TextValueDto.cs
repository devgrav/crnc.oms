using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crnc.Oms.Production.Domain.Dto
{
    public class TextValueDto<Tkey,TValue>
    {
        public Tkey Value { get; set; }

        public TValue Text { get; set; }
    }
}
