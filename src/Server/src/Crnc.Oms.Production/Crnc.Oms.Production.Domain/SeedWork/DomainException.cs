using System;

namespace Crnc.Oms.Production.Domain.SeedWork
{
    public class DomainException
        : Exception
    {
        public DomainException()
        {
            
        }

        public DomainException(string message)
            :base(message)
        {
            
        }
    }
}