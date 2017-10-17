using Crnc.Oms.Domain.Aggregates.Users;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Crnc.Oms.DataAccess.DbInitialize
{
    class DataContextDbInitializer
        : DropCreateDatabaseAlways<DataContext>
    {
        protected override void Seed(DataContext dbContext)
        {
            try
            {
                var photos = GetUserPhotos();

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
                    User.CreateNew("jack_richer","111111","Jack","Richer","jack_richer@crnc.com",null, dbContext.Roles.First(r=> r.Title.Equals("Admin")),photos[0]),
                    User.CreateNew("shon_bean","111111","Shon","Bean","shon_bean@crnc.com",null,dbContext.Roles.First(r=> r.Title.Equals("Main manager")),photos[1]),
                    User.CreateNew("helen_smith","111111","Helen","Smith","helen_smith@crnc.com",null,dbContext.Roles.First(r=> r.Title.Equals("Manager")),photos[2]),
                    User.CreateNew("agness_stuart","111111","Agness","Stuart","agness_stuart@crnc.com",null,dbContext.Roles.First(r=> r.Title.Equals("Manager")),photos[3])
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

        private List<UserPhoto> GetUserPhotos()
        {
            var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(r => r.Contains("UserPhotos"));

            var photos = new List<UserPhoto>();
            using (var streamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)))
            {
                var xml = streamReader.ReadToEnd();
                var xdoc = XDocument.Parse(xml);                
                var userPhotos = xdoc.Descendants().Where(n => n.Name == "UserPhoto").ToList();
                foreach (var photo in userPhotos)
                {                    
                    var mimeType = photo.Descendants().Where(n => n.Name == "MimeType").Select(n => n.Value).FirstOrDefault();
                    var content = photo.Descendants().Where(n => n.Name == "Content").Select(n => Convert.FromBase64String(n.Value)).FirstOrDefault();

                    photos.Add(new UserPhoto
                    {
                        MimeType = mimeType,
                        Content = content
                    });
                }
            }

            return photos;
        }       
    }
}
