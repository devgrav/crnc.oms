﻿namespace Crnc.Oms.Sales.Domain.SeedWork
{
    public interface ICurrentUserContext
    {
        string AuthToken { get; }
        
        string FirstName { get; }
        
        string LastName { get; }
        
        string FullName { get; }
        
        string Login { get; }
        
        bool IsAnonymous { get; }
    }
}