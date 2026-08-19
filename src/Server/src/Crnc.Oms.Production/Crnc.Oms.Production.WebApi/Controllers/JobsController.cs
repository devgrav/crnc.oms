using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Production.Application.Exceptions;
using Crnc.Oms.Production.Application.Services.Abstractions;
using Crnc.Oms.Production.Domain.Aggregates.JobAggregate;
using Crnc.Oms.Production.Domain.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crnc.Oms.Production.WebApi.Controllers
{
    [Produces("application/json")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }
        
        /// <summary>
        /// Get jobs for table
        /// </summary>
        /// <remarks>Returns jobs for table</remarks>
        /// <response code="200">Returned jobs</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<GetJobsForListDto> Get(CancellationToken  cancellationToken = default)
        {
            return await _jobService.GetJobsForList();
        }
        
        /// <summary>
        /// Get jobs for table
        /// </summary>
        /// <remarks>Returns jobs for table</remarks>
        /// <response code="200">Returned jobs</response>
        /// <response code="404">Not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id, CancellationToken  cancellationToken = default)
        {
            try
            {
                return Ok(await _jobService.GetJob(id));
            }
            catch (MissingEntityException)
            {
                return NotFound();
            }
        }
        
        /// <summary>
        /// Finish job
        /// </summary>
        /// <remarks>Edit order</remarks>
        /// <response code="200">Job edited</response>
        /// <response code="400">Not valid</response>
        /// <response code="404">Not found</response>
        [HttpPut("{id}/finished")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Finish(Guid id, CancellationToken  cancellationToken = default)
        {
            try
            {
                await _jobService.FinishJob(id);
                
                return Ok();
            }
            catch(MissingEntityException)
            {
                return NotFound();
            }
        }
        
        /// <summary>
        /// Change Priority
        /// </summary>
        /// <remarks>Edit order</remarks>
        /// <response code="200">Job edited</response>
        /// <response code="400">Not valid</response>
        /// <response code="404">Not found</response>
        [HttpPut("{id}/priority")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePriority(Guid id, [FromBody] ChangePriorityDto dto, CancellationToken  cancellationToken = default)
        {
            try
            {
                await _jobService.ChangePriority(id, dto.Priority);
                
                return Ok();
            }
            catch(MissingEntityException)
            {
                return NotFound();
            }
        }
    }
}