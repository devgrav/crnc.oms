using System;
using System.Threading.Tasks;
using Crnc.Oms.Messaging.Contract.Commands;
using Crnc.Oms.Messaging.Contract.Events;
using Crnc.Oms.Sales.Application;
using Crnc.Oms.Sales.Application.Features.Orders.Commands;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Input;
using MassTransit;

namespace Crnc.Oms.Sales.WebApi.Consumers
{
    public class JobCreatedForOrderConsumer:
            IConsumer<JobCreatedForOrderEvent>
        {
            private readonly ICommandQueryDispatcher _dispatcher;

            public JobCreatedForOrderConsumer( ICommandQueryDispatcher dispatcher)
            {
                _dispatcher = dispatcher;
            }

            public async Task Consume(ConsumeContext<JobCreatedForOrderEvent> context)
            {
                var notification = context.Message;
                
                if(notification == null)
                    throw new ArgumentNullException(nameof(notification));

                await _dispatcher.Dispatch(new SetOrderJobInfoInputDto()
                {
                    JobId = notification.JobId,
                    JobNumber = notification.JobNumber,
                    OrderId = notification.OrderId
                });
            }
        }
    
}