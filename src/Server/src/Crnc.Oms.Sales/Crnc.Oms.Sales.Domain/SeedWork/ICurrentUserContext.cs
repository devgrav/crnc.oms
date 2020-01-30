﻿using System;
 using Crnc.Oms.Sales.Domain.Aggregates.Order;

 namespace Crnc.Oms.Sales.Domain.SeedWork
{
    public interface ICurrentUserContext
    {
        Guid Id { get; }
        
        string AuthToken { get; }

        FullName FullName { get; }
        
        string Login { get; }
        
        Email Email { get; }
        
        bool IsAnonymous { get; }
    }
}