using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application;
using Crnc.Oms.Sales.Application.Features.Orders.Dto;
using Crnc.Oms.Sales.DataAccess;
using Crnc.Oms.Sales.Domain.Aggregates.Orders;
using Microsoft.AspNetCore.Mvc;

namespace Crnc.Oms.Sales.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IUseCaseQueryHandler<OrdersForTableRequestDto, OrdersForTableResponseDto> _getOrdersQueryHandler;

        public OrdersController(IUseCaseQueryHandler<OrdersForTableRequestDto, OrdersForTableResponseDto> getOrdersQueryHandler)
        {
            _getOrdersQueryHandler = getOrdersQueryHandler;
        }
        
        // GET
        [HttpGet]
        public async Task<OrdersForTableResponseDto> Get()
        {
            var orders = await _getOrdersQueryHandler.HandleAsync(new OrdersForTableRequestDto());
            return orders;
        }
    }
}