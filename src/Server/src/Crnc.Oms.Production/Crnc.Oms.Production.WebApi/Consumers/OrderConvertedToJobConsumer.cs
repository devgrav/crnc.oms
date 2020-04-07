using System;
using System.Threading.Tasks;
using Crnc.Oms.Messaging.Contract.Events;
using Crnc.Oms.Production.Application.Services;
using Crnc.Oms.Production.Application.Services.Abstractions;
using Crnc.Oms.Production.Domain.Aggregates.JobAggregate;
using Crnc.Oms.Production.Domain.Dto;
using MassTransit;

namespace Crnc.Oms.Production.WebApi.Consumers
{
    public class OrderConvertedToJobConsumer
        : IConsumer<OrderConvertedToJobEvent>
    {
        private readonly IJobService _jobService;

        public OrderConvertedToJobConsumer(IJobService jobService)
        {
            _jobService = jobService;
        }

        public async Task Consume(ConsumeContext<OrderConvertedToJobEvent> context)
        {
            var notification = context.Message;
                
            if(notification == null)
                throw new ArgumentNullException(nameof(notification));

            await _jobService.CreateJob(new CreateJobDto()
            {
                JobDescription = notification.JobDescription,
                JobType = Enum.Parse<JobType>(notification.JobType),
                ManagerLogin = notification.ManagerLogin,
                MaterialSource = Enum.Parse<MaterialSource>(notification.MaterialSource),
                OrderId = notification.OrderId,
                OrderNumber = notification.OrderNumber,
                ManagerFullName = notification.ManagerFullName
            });
        }
    }
}