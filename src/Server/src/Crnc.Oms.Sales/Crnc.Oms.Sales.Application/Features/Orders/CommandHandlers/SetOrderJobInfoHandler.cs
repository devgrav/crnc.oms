using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Input;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Repositories;

namespace Crnc.Oms.Sales.Application.Features.Orders.Commands
{
    public class SetOrderJobInfoHandler
        : IUseCaseCommandHandler<SetOrderJobInfoInputDto, EmptyOutputDto>
    {
        private readonly IOrderRepository _orderRepository;

        public SetOrderJobInfoHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        
        public async Task<EmptyOutputDto> Handle(SetOrderJobInfoInputDto request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.FindByIdAsync(request.OrderId);
            
            order.SetInfoAboutJob(new JobInfo(order.JobInfo.Type, order.JobInfo.Description, request.JobId, request.JobNumber));

            await _orderRepository.SaveChangesAsync();
            
            return new EmptyOutputDto();
        }
    }
}