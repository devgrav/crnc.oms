using System;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    public class Manager
        : DomainEntity
    {
        public FullName FullName { get; set; }

        public string Login { get; set; }

        public Email Email { get; set; }
        
        public Manager(FullName fullName, Email email, string login, Guid id)
            : base(id)
        {
            FullName = fullName;
            Email = email;
            Login = login;
        }

        protected Manager()
        {
            
        }
    }
}