using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Crnc.Oms.Sales.Application
{
    /// <summary>
    /// Handler of query for user scenario
    /// </summary>
    /// <typeparam name="TQuery">Type for query data</typeparam>
    /// <typeparam name="TOut">Type for query result</typeparam>
    public interface IUseCaseQueryHandler<TQuery, TOut>
        : IRequestHandler<TQuery, TOut>
     where TQuery: IUseCaseQuery<TOut>
    {
    }
}