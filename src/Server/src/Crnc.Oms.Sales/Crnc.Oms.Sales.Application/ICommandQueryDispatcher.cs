using System.Threading;
using System.Threading.Tasks;

namespace Crnc.Oms.Sales.Application
{
    public interface ICommandQueryDispatcher
    {
        Task<TOut> Dispatch<TOut>(IUseCaseCommand<TOut> command, CancellationToken cancellationToken = default);
        
        Task<TOut> Dispatch<TOut>(IUseCaseQuery<TOut> query, CancellationToken cancellationToken = default);
    }
}