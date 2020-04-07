using System;

namespace Crnc.Oms.Production.Domain.SeedWork
{
    public interface ICurrentDateTimeProvider
    {
        DateTime GetNow();

        DateTime GetToday();
    }
}