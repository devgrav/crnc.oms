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

                var roles = new List<Role>()
                {
                    new Role("Admin"),
                    new Role("Main manager"),
                    new Role("Manager"),
                };

                dbContext.Roles.AddRange(roles);
                dbContext.SaveChanges();

                var users = new List<User>()
                {
                    User.CreateNew("jack_richer","111111","Jack","Richer","jack_richer@crnc.com",dbContext.Roles.First(r=> r.Title.Equals("Admin"))),
                    User.CreateNew("shon_bean","111111","Shon","Bean","shon_bean@crnc.com",dbContext.Roles.First(r=> r.Title.Equals("Main manager"))),
                    User.CreateNew("helen_smith","111111","Helen","Smith","helen_smith@crnc.com",dbContext.Roles.First(r=> r.Title.Equals("Manager"))),
                    User.CreateNew("agness_stuart","111111","Agness","Stuart","agness_stuart@crnc.com",dbContext.Roles.First(r=> r.Title.Equals("Manager")))
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
