using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto.Output
{
    public class TextValueOutputDto<Tkey,TValue>
    {
        public Tkey Value { get; set; }

        public TValue Text { get; set; }
    }
}
