using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application;
using Crnc.Oms.Sales.Application.Features.Orders.Dto;
using Crnc.Oms.Sales.DataAccess;
using Crnc.Oms.Sales.Domain.Aggregates.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crnc.Oms.Sales.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IUseCaseQueryHandler<OrdersForTableInputDto, OrdersForTableOutputDto> _getOrdersQueryHandler;

        public OrdersController(IUseCaseQueryHandler<OrdersForTableInputDto, OrdersForTableOutputDto> getOrdersQueryHandler)
        {
            _getOrdersQueryHandler = getOrdersQueryHandler;
        }
        
        // GET
        [HttpGet]
        public async Task<OrdersForTableOutputDto> Get()
        {
            var orders = await _getOrdersQueryHandler.HandleAsync(new OrdersForTableInputDto());
            return orders;
        }
    }
}