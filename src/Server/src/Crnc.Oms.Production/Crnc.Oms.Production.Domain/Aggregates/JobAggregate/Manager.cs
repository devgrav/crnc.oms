using System;
using Crnc.Oms.Production.Domain.SeedWork;

namespace Crnc.Oms.Production.Domain.Aggregates.JobAggregate
{
    public class Manager
        : IValueObject
    {
        public string FullName { get; private set; }

        public string Login { get; private set; }

        public Manager(string fullName, string login)
        {
            FullName = fullName;
            Login = login;
        }

        protected Manager()
        {
            
        }
    }
}