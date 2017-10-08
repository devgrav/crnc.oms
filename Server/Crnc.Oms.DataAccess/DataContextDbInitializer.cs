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
                    User.CreateNew("admin","111111","Jack","Richer","jack_richer@crnc.com"),
                    User.CreateNew("manager","111111","Shon","Bean","shon_bean@crnc.com"),
                    User.CreateNew("designer","111111","Brad","Peat","jack_peat@crnc.com")
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
                        $"Entity of type \"{eve.Entry.Entity.GetType().Name}\" in state \"{eve.Entry.State}\" has validation errors:");

                    foreach (var ve in eve.ValidationErrors)
                        stringBuilder.AppendLine(
                           $"- Property: \"{ve.PropertyName}\", Value: \"{eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName)}\", Error: \"{ve.ErrorMessage}\"");

                    var message = stringBuilder.ToString();
                    Debug.WriteLine(message);
                }
            }
        }
    }
}
