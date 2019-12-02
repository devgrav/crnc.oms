using System;

namespace Crnc.Oms.Security.Domain.SeedWork
{
    public interface ICurrentDateTimeProvider
    {
        DateTime GetNow();

        DateTime GetToday();
    }
}