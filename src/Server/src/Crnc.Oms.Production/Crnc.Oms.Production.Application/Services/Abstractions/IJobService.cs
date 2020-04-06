using System;
using System.Threading.Tasks;
using Crnc.Oms.Production.Domain.Aggregates.JobAggregate;
using Crnc.Oms.Production.Domain.Dto;

namespace Crnc.Oms.Production.Application.Services.Abstractions
{
    public interface IJobService
    {
        Task<GetJobsForListDto> GetJobsForList();
        
        Task<GetJobDto> GetJob(Guid id);

        Task<Guid> CreateJob(CreateJobDto dto);

        Task FinishJob(Guid id);
        
        Task ChangePriority(Guid id, Priority priority);
    }
}