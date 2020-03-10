using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Crnc.Oms.Sales.Application
{
    public class CommandQueryDispatcher
        : ICommandQueryDispatcher
    {
        private readonly IMediator _mediator;

        public CommandQueryDispatcher(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        public Task<TOut> Dispatch<TOut>(IUseCaseCommand<TOut> command, CancellationToken cancellationToken = default)
        {
            return _mediator.Send(command, cancellationToken);
        }

        public Task<TOut> Dispatch<TOut>(IUseCaseQuery<TOut> query, CancellationToken cancellationToken = default)
        {
            return _mediator.Send(query, cancellationToken);
        }
    }
}