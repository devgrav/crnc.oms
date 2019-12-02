using System;

namespace Crnc.Oms.Sales.Domain.SeedWork
{
    public interface ICurrentDateTimeProvider
    {
        DateTime GetNow();

        DateTime GetToday();
    }
}