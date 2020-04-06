﻿using System;
 using Crnc.Oms.Production.Domain.Aggregates.Order;
 using Crnc.Oms.Production.Domain.Aggregates.JobAggregate;

 namespace Crnc.Oms.Production.Domain.SeedWork
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