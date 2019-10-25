using System.Collections.Generic;
using System.Linq;
using Crnc.Oms.Sales.DataAccess;
using Crnc.Oms.Sales.Domain.Aggregates.Orders;
using Microsoft.AspNetCore.Mvc;

namespace Crnc.Oms.Sales.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly SalesDataContext _context; 
        
        public OrdersController(SalesDataContext context)
        {
            _context = context;
        }
        
        // GET
        [HttpGet]
        public IEnumerable<Order> Get()
        {
            var orders = _context.Orders.ToList();
            return orders;
        }
    }
}