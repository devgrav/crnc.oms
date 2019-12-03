using Crnc.Oms.Security.Domain.Aggregates.Users;
using Crnc.Oms.Security.Infrastructure.CrossCutting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Crnc.Oms.Security.Infrastructure.DataAccess.Data
{
    class DataFactory
    {
        public static List<Role> GetRoles()
        {
            var roles = new List<Role>()
            {
                new Role("Admin",Guid.Parse("0c7871c3-4751-4af6-b0ef-21c38064e9f2")),
                new Role("Main manager", Guid.Parse("f1ba72d8-5ebc-4cc4-8b31-eaa0baa87293")),
                new Role("Manager",Guid.Parse("29679868-fcfe-4350-913d-526a54ea896d")),
            };

            return roles;
        }

        public static List<User> GetUsers(List<Role> roles)
        {
                var password = PasswordHelper.GetHash("111111");
                var photos = GetUserPhotos();

                var users = new List<User>()
                {
                    User.CreateNew("admin",password.Hash, password.Salt,"John","Admin","admin@crnc.com",null, roles.First(r=> r.Title.Equals("Admin")),null, Guid.Parse("2a89985f-f013-4f2a-9545-395efb43a142")),
                    User.CreateNew("jack_richer",password.Hash, password.Salt,"Jack","Richer","jack_richer@crnc.com",null, roles.First(r=> r.Title.Equals("Admin")),photos[0]),
                    User.CreateNew("shon_bean",password.Hash, password.Salt,"Shon","Bean","shon_bean@crnc.com",null,roles.First(r=> r.Title.Equals("Main manager")),photos[1]),
                    User.CreateNew("helen_smith",password.Hash, password.Salt,"Helen","Smith","helen_smith@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),photos[2]),
                    User.CreateNew("agness_stuart",password.Hash, password.Salt,"Agness","Stuart","agness_stuart@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),photos[3]),
                    User.CreateNew("darius_larson",password.Hash, password.Salt,"Darius","Larson","darius_larson@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null),
                    User.CreateNew("gillian_labadie",password.Hash, password.Salt,"Gillian","Labadie","darius_larson@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null),
                    User.CreateNew("jonas_nolan",password.Hash, password.Salt,"Jonas","Nolan","jonas_nolan@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null),
                    User.CreateNew("harvey_denesik",password.Hash, password.Salt,"Harvey","Denesik","harvey_denesik@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null),
                    User.CreateNew("jordon_ortiz",password.Hash, password.Salt,"Jordon","Ortiz","jordon_ortiz@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null),
                    User.CreateNew("brook_dach",password.Hash, password.Salt,"Brook","Dack","brook_dach@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null),
                    User.CreateNew("kiel_jones",password.Hash, password.Salt,"Kiel","Jones","kiel_jones@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null)

                };

            return users;
        }

        private static List<UserPhoto> GetUserPhotos()
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