using System;
using System.Threading;
using System.Threading.Tasks;

namespace Crnc.Oms.Sales.Application
{
    /// <summary>
    /// Handler of query for user scenario
    /// </summary>
    /// <typeparam name="TQuery">Type for query data</typeparam>
    /// <typeparam name="TOut">Type for query result</typeparam>
    public interface IUseCaseQueryHandler<TQuery, TOut>
     where TQuery: IUseCaseQuery<TOut>
    {
        /// <summary>
        /// Async handle query
        /// </summary>
        /// <param name="queryData">Data of query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<TOut> HandleAsync(TQuery queryData, CancellationToken cancellationToken=default);
    }
}