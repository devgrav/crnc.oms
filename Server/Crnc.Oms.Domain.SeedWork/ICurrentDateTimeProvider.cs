using System;

namespace Crnc.Oms.Domain.SeedWork
{
    public interface ICurrentDateTimeProvider
    {
        DateTime GetNow();

        DateTime GetToday();
    }
}