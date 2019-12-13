using System;

namespace Crnc.Oms.Security.Infrastructure.DataAccess.Exceptions
{
    public class EntityAlreadyExistedException
        : ApplicationException
    {
        public EntityAlreadyExistedException(string message = "Entity has already existed")
            : base(message)
        {

        }
    }
}