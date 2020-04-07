using System;

namespace Crnc.Oms.Production.Domain.SeedWork
{
    public class CurrentDateTimeProvider
        : ICurrentDateTimeProvider
    {
        public DateTime GetNow()
        {
            return  DateTime.Now;
            ;
        }

        public DateTime GetToday()
        { 
            return DateTime.Today;
        }
    }
}