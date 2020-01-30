using System;
using System.Collections.Generic;

namespace Crnc.Oms.Sales.Domain.SeedWork
{
    /// <summary>
    /// Base class for all entities
    /// </summary>
    public class DomainEntity
    {
        /// <summary>
        /// Id of entity
        /// </summary>
        public Guid Id { get; protected set; }

        private readonly List<DomainEvent> _domainEvents = new List<DomainEvent>();
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();


        public DomainEntity(Guid id)
        {
            Id = id;
        }

        protected DomainEntity()
        {
            
        }

        #region Identity Management
        public static bool operator ==(DomainEntity e1, DomainEntity e2)
        {
            // Both null or same instance
            if (ReferenceEquals(e1, e2))
                return true;

            // Return false if one is null, but not both 
            if (((object)e1 == null) || ((object)e2 == null))
                return false;

            return e1.Equals(e2);
        }

        public static bool operator !=(DomainEntity e1, DomainEntity e2)
        {
            return !(e1 == e2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var entity = obj as DomainEntity;

            if (entity == null)
                return false;

            if (this == entity)
                return true;

            return Id == entity.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        #endregion

        public void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
        
        public void RemoveDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }
    }
}
