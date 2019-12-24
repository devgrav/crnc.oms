using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application.Exceptions;
using Crnc.Oms.Sales.Application.Features.Orders.Dto;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Input;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Repositories;

namespace Crnc.Oms.Sales.Application.Features.Orders.Commands
{
    public class EditOrder
        : IUseCaseCommandHandler<EditOrderInputDto, EmptyOutputDto>
    {
        private readonly IOrderRepository _orderRepository;

        public EditOrder(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        
        public async Task<EmptyOutputDto> HandleAsync(EditOrderInputDto command, CancellationToken cancellationToken = default)
        {
            if(command == null)
                throw new ArgumentNullException(nameof(command));

            var order = await _orderRepository.FindByIdAsync(command.Id, cancellationToken);
            
            if(order == null)
                throw new MissingEntityException("Order not found");
            
            var customer = new Customer(new FullName
                (
                    command.FirstName,
                    command.LastName,
                    command.MiddleName
                ),
                new NameAbbreviation(command.Abbreviation), 
                new Email(command.Email),
                new Phone(command.Phone)
            );
            
            order.Edit(command.JobType, 
                command.JobDescription, 
                command.MaterialSource, 
                command.SignOffType, 
                customer);
            
            order.ChangeStatus(command.Status);

            await _orderRepository.SaveChangesAsync(cancellationToken);
            
            return new EmptyOutputDto();
        }
    }
}