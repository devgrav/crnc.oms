using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application;
using Crnc.Oms.Sales.Application.Exceptions;
using Crnc.Oms.Sales.Application.Features.Orders.Dto;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Input;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;
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
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ICommandQueryDispatcher _dispatcher;

        public OrdersController(ICommandQueryDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }
        
        /// <summary>
        /// Get orders for table
        /// </summary>
        /// <remarks>Returns orders for table</remarks>
        /// <response code="200">Returned orders</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<GetOrdersForTableOutputDto> Get(CancellationToken  cancellationToken = default)
        {
            return await _dispatcher.Dispatch(new GetOrdersForTableInputDto(),cancellationToken);
        }
        
        /// <summary>
        /// Get orders for table
        /// </summary>
        /// <remarks>Returns orders for table</remarks>
        /// <response code="200">Returned orders</response>
        /// <response code="401">Not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(Guid id, CancellationToken  cancellationToken = default)
        {
            try
            {
                return Ok(await _dispatcher.Dispatch(new GetOrderInputDto()
                {
                    Id =  id
                },cancellationToken));
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
        public async Task<GetNewOrderOutputDto> GetNew(CancellationToken  cancellationToken = default)
        {
            return await _dispatcher.Dispatch(new GetNewOrderInputDto(), cancellationToken);
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
        public async Task<CreateOrderOutputDto> Create([FromBody]CreateOrderInputDto dto, CancellationToken  cancellationToken = default)
        {
            return await _dispatcher.Dispatch(dto, cancellationToken);
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
        public async Task<IActionResult> Edit([FromBody]EditOrderInputDto dto, CancellationToken  cancellationToken = default)
        {
            try
            {
                await _dispatcher.Dispatch(dto, cancellationToken);
                
                return Ok();
            }
            catch(MissingEntityException)
            {
                return NotFound();
            }
        }
    }
}