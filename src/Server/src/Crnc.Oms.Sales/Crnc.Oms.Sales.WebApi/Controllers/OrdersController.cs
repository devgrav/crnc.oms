using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application;
using Crnc.Oms.Sales.Application.Exceptions;
using Crnc.Oms.Sales.Application.Features.Orders.Dto;
using Crnc.Oms.Sales.DataAccess;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
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
        private readonly IUseCaseQueryHandler<GetOrderInputDto, GetOrderOutputDto> _getOrderQueryHandler;
        private readonly IUseCaseQueryHandler<GetNewOrderInputDto, GetNewOrderOutputDto> _getNewOrderQueryHandler;
        private readonly IUseCaseCommandHandler<CreateOrderInputDto> _createOrderCommandHandler;
        private readonly IUseCaseCommandHandler<EditOrderInputDto> _editOrderCommandHandler;

        public OrdersController(
            IUseCaseQueryHandler<GetOrdersForTableInputDto, GetOrdersForTableOutputDto> getOrdersQueryHandler, 
            IUseCaseQueryHandler<GetNewOrderInputDto, GetNewOrderOutputDto> getNewOrderQueryHandler,
            IUseCaseQueryHandler<GetOrderInputDto, GetOrderOutputDto> getOrderQueryHandler,
            IUseCaseCommandHandler<CreateOrderInputDto> createOrderCommandHandler, 
            IUseCaseCommandHandler<EditOrderInputDto> editOrderCommandHandler)
        {
            _getOrdersQueryHandler = getOrdersQueryHandler;
            _getNewOrderQueryHandler = getNewOrderQueryHandler;
            _createOrderCommandHandler = createOrderCommandHandler;
            _editOrderCommandHandler = editOrderCommandHandler;
            _getOrderQueryHandler = getOrderQueryHandler;
        }
        
        /// <summary>
        /// Get orders for table
        /// </summary>
        /// <remarks>Returns orders for table</remarks>
        /// <response code="200">Returned orders</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<GetOrdersForTableOutputDto> Get(GetOrdersForTableInputDto dto, CancellationToken  cancellationToken = default)
        {
            return await _getOrdersQueryHandler.HandleAsync(dto,cancellationToken);
        }
        
        /// <summary>
        /// Get orders for table
        /// </summary>
        /// <remarks>Returns orders for table</remarks>
        /// <response code="200">Returned orders</response>
        /// <response code="401">Not found</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(GetOrderInputDto dto, CancellationToken  cancellationToken = default)
        {
            try
            {
                return Ok(await _getOrderQueryHandler.HandleAsync(dto,cancellationToken));
            }
            catch (MissingEntityException)
            {
                return NotFound();
            }
        }
        
        /// <summary>
        /// Get new order
        /// </summary>
        /// <remarks>Returns new order for create</remarks>
        /// <response code="200">Returned new order</response>
        [HttpGet("new")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<GetNewOrderOutputDto> Get(GetNewOrderInputDto dto, CancellationToken  cancellationToken = default)
        {
            return await _getNewOrderQueryHandler.HandleAsync(dto,cancellationToken);
        }
        
        /// <summary>
        /// Create order
        /// </summary>
        /// <remarks>Create new order</remarks>
        /// <response code="200">Order created</response>
        /// <response code="400">Not valid</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task Create(CreateOrderInputDto dto, CancellationToken  cancellationToken = default)
        {
            await _createOrderCommandHandler.HandleAsync(dto, cancellationToken);
        }
        
        /// <summary>
        /// Edit order
        /// </summary>
        /// <remarks>Edit order</remarks>
        /// <response code="200">Order edited</response>
        /// <response code="400">Not valid</response>
        /// <response code="404">Not found</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(EditOrderInputDto dto, CancellationToken  cancellationToken = default)
        {
            try
            {
                await _editOrderCommandHandler.HandleAsync(dto, cancellationToken);
                
                return Ok();
            }
            catch(MissingEntityException)
            {
                return NotFound();
            }
        }
    }
}