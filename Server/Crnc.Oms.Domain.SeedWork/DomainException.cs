using System;

namespace Crnc.Oms.Domain.SeedWork
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