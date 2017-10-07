using Crnc.Oms.Domain.Aggregates.Users;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crnc.Oms.DataAccess
{
    class DataContextDbInitializer
        : DropCreateDatabaseAlways<DataContext>
    {
        protected override void Seed(DataContext dbContext)
        {
            try
            {
                var users = new List<User>()
                {
                    User.CreateNew("admin","111111","Jack","Richer"),
                    User.CreateNew("manager","111111","Shon","Beam"),
                    User.CreateNew("designer","111111","Brad","Peat")
                };

                dbContext.Users.AddRange(users);

                dbContext.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(
                        $"Сущность типа \"{eve.Entry.Entity.GetType().Name}\" в состоянии \"{eve.Entry.State}\" имеет следующие ошибки валидации:");

                    foreach (var ve in eve.ValidationErrors)
                        stringBuilder.AppendLine(
                            $"- Свойство: \"{ve.PropertyName}\", Значение: \"{eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName)}\", Ошибка: \"{ve.ErrorMessage}\"");

                    var message = stringBuilder.ToString();
                    Debug.WriteLine(message);
                }
            }
        }
    }
}
