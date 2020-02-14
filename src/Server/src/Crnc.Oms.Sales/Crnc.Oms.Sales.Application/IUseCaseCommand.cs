using MediatR;

namespace Crnc.Oms.Sales.Application
{
    public interface IUseCaseCommand<TOut>
        : IRequest<TOut>
    {
        
    }
}