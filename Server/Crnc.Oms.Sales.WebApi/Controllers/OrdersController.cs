using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application;
using Crnc.Oms.Sales.Application.Features.Orders.Dto;
using Crnc.Oms.Sales.DataAccess;
using Crnc.Oms.Sales.Domain.Aggregates.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crnc.Oms.Sales.WebApi.Controllers
{
    [Produces("application/json")]
    [Authorize]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IUseCaseQueryHandler<GetOrdersForTableInputDto, GetOrdersForTableOutputDto> _getOrdersQueryHandler;
        private readonly IUseCaseQueryHandler<GetNewOrderInputDto, GetNewOrderOutputDto> _getNewOrderQueryHandler;

        public OrdersController(IUseCaseQueryHandler<GetOrdersForTableInputDto, GetOrdersForTableOutputDto> getOrdersQueryHandler, 
            IUseCaseQueryHandler<GetNewOrderInputDto, GetNewOrderOutputDto> getNewOrderQueryHandler)
        {
            _getOrdersQueryHandler = getOrdersQueryHandler;
            _getNewOrderQueryHandler = getNewOrderQueryHandler;
        }
        
        /// <summary>
        /// Get orders for table
        /// </summary>
        /// <remarks>Returns orders for table</remarks>
        /// <response code="200">Returned orders</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<GetOrdersForTableOutputDto> Get(GetOrdersForTableInputDto dto)
        {
            return await _getOrdersQueryHandler.HandleAsync(dto);
        }
        
        /// <summary>
        /// Get new order
        /// </summary>
        /// <remarks>Returns new order for create</remarks>
        /// <response code="200">Returned new order</response>
        [HttpGet("new")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<GetNewOrderOutputDto> Get(GetNewOrderInputDto dto)
        {
            return await _getNewOrderQueryHandler.HandleAsync(dto);
        }
    }
}