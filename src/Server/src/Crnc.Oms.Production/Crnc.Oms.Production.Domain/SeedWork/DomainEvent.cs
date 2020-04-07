using System;
using MediatR;

namespace Crnc.Oms.Production.Domain.SeedWork
{
    public class DomainEvent
        : INotification
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