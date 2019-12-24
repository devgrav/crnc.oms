using System.Threading;
using System.Threading.Tasks;

namespace Crnc.Oms.Sales.Application
{
    /// <summary>
    /// Handler of command for user scenario
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    public interface IUseCaseCommandHandler<TIn, TOut>
        where TIn: IUseCaseCommand<TOut>

    {
        /// <summary>
        /// Handle command async
        /// </summary>
        /// <param name="command">Data of command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<TOut> HandleAsync(TIn command, CancellationToken cancellationToken=default); 
    }
}