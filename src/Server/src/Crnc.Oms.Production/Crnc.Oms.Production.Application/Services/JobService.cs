using System;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Production.Application.Exceptions;
using Crnc.Oms.Production.Application.Helpers;
using Crnc.Oms.Production.Application.Services.Abstractions;
using Crnc.Oms.Production.Domain.Aggregates.JobAggregate;
using Crnc.Oms.Production.Domain.Dto;
using Crnc.Oms.Production.Domain.Gateways;
using Crnc.Oms.Production.Domain.Repositories;
using Crnc.Oms.Production.Domain.SeedWork;

namespace Crnc.Oms.Production.Application.Services
{
    public class JobService
        : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly ICurrentDateTimeProvider _currentDateTimeProvider;
        private readonly ISalesOrderGateway _salesOrderGateway;

        public JobService(IJobRepository jobRepository, ICurrentDateTimeProvider currentDateTimeProvider, ISalesOrderGateway salesOrderGateway)
        {
            _jobRepository = jobRepository;
            _currentDateTimeProvider = currentDateTimeProvider;
            _salesOrderGateway = salesOrderGateway;
        }
        
        public async Task<GetJobsForListDto> GetJobsForList()
        {
            var jobs = await _jobRepository.FindAllAsync();

            var items = jobs.Select(x => new JobListItemDto()
            {
                Id = x.Id,
                Number = x.Number,
                Manager = $"{x.Manager.FullName} ({x.Manager.Login})",
                Priority = EnumHelper.GetDescription(x.Priority),
                JobDescription = x.JobDescription,
                JobType = EnumHelper.GetDescription(x.JobType),
                MaterialSource = EnumHelper.GetDescription(x.MaterialSource),
                PriorityEnum = x.Priority,
                IsJobCompeted = x.IsJobCompeted
            }).ToList();

            return new GetJobsForListDto()
            {
                Items = items
            };
        }

        public async Task<GetJobDto> GetJob(Guid id)
        {
            var job = await _jobRepository.FindByIdAsync(id);
            
            if(job == null)
                throw new MissingEntityException("Job not found");
            
            var jobDto =  new GetJobDto()
            {
                Id = job.Id,
                Number = job.Number,
                Manager = $"{job.Manager.FullName} ({job.Manager.Login})",
                DateCreated = job.DateCreated.ToStandartFormatWithTime(),
                JobDescription = job.JobDescription,
                JobType = EnumHelper.GetDescription(job.JobType),
                MaterialSource = EnumHelper.GetDescription(job.MaterialSource),
                PriorityEnum = job.Priority,
                Priorities = EnumHelper.ToDictionaryWithKeysAndDescriptions(Priority.Low)
                    .Select(x => new TextValueDto<int, string>()
                    {
                        Text = x.Value,
                        Value = x.Key
                    }).ToList(),
                IsJobCompeted = job.IsJobCompeted
            };

            return jobDto;
        }

        public async Task<Guid> CreateJob(CreateJobDto dto)
        {
            var job = await _jobRepository.GetJobByOrderIdAsync(dto.OrderId);

            if (job != null)
            {
                return job.Id;
            }
            
            job = new Job(
                dto.OrderId, 
                _currentDateTimeProvider.GetNow(), 
                dto.JobType, 
                dto.JobDescription, 
                dto.MaterialSource,
                new Manager(
                    dto.ManagerFullName, 
                    dto.ManagerLogin), 
                dto.OrderId, 
                dto.OrderNumber);
            
            _jobRepository.Add(job);

            await _jobRepository.SaveChangesAsync();

            await _salesOrderGateway.NotifyThatJobForOrderCreatedAsync(job.Id, job.Number, job.OrderId);

            return job.Id;
        }

        public async Task FinishJob(Guid id)
        {
            var job = await _jobRepository.FindByIdAsync(id);
            
            job.FinishJob();

            await _jobRepository.SaveChangesAsync();
        }

        public async Task ChangePriority(Guid id, Priority priority)
        {
            var job = await _jobRepository.FindByIdAsync(id);
            
            job.ChangePriority(priority);

            await _jobRepository.SaveChangesAsync();
        }
    }
}