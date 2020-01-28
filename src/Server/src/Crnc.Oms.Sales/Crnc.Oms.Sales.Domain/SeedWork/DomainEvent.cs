using System;

namespace Crnc.Oms.Sales.Domain.SeedWork
{
    public class DomainEvent
    {
        public Guid Id { get; protected set; }

        public string Name { get; protected  set; }
        
        public DateTime CreatedDate { get; protected  set; }
        
        public string Event { get; set; }

        public DomainEvent()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            Name = GetType().Name;
        }
    }
}