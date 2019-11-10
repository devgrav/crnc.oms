using System.Threading;
using System.Threading.Tasks;

namespace Crnc.Oms.Sales.Application
{
    /// <summary>
    /// Handler of command for user scenario
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    public interface IUseCaseCommandHandler<TOut>

    {
        /// <summary>
        /// Handle command async
        /// </summary>
        /// <param name="command">Data of command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task HandleAsync(IUseCaseCommand command, CancellationToken cancellationToken=default); 
    }
}