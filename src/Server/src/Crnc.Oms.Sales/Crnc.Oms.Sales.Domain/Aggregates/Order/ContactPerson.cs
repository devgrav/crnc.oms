using System;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    public class ContactPerson
        : IValueObject
    {
        public FullName FullName { get; private set; }

        public Email Email { get; private set; }
        
        public Phone Phone { get; private set; }
        
        public ContactPerson(FullName fullName, Email email, Phone phone)
        {
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Phone = phone ?? throw new ArgumentNullException(nameof(phone));
        }

        protected ContactPerson()
        {
            
        }
    }
}