using Crnc.Oms.Domain.Aggregates.Users;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Crnc.Oms.DataAccess.Data
{
    class DataFactory
    {
        public static List<Role> GetRoles()
        {
            var roles = new List<Role>()
            {
                new Role("Admin"),
                new Role("Main manager"),
                new Role("Manager"),
            };

            return roles;
        }

        public static List<User> GetUsers(List<Role> roles)
        {
                //var photos = GetUserPhotos();

                var users = new List<User>()
                {
                    User.CreateNew("jack_richer","111111","Jack","Richer","jack_richer@crnc.com",null, roles.First(r=> r.Title.Equals("Admin")),null),
                    User.CreateNew("shon_bean","111111","Shon","Bean","shon_bean@crnc.com",null,roles.First(r=> r.Title.Equals("Main manager")),null),
                    User.CreateNew("helen_smith","111111","Helen","Smith","helen_smith@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null),
                    User.CreateNew("agness_stuart","111111","Agness","Stuart","agness_stuart@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null),
                    User.CreateNew("darius_larson","111111","Darius","Larson","darius_larson@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null),
                    User.CreateNew("gillian_labadie","111111","Gillian","Labadie","darius_larson@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null),
                    User.CreateNew("jonas_nolan","111111","Jonas","Nolan","jonas_nolan@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null),
                    User.CreateNew("harvey_denesik","111111","Harvey","Denesik","harvey_denesik@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null),
                    User.CreateNew("jordon_ortiz","111111","Jordon","Ortiz","jordon_ortiz@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null),
                    User.CreateNew("brook_dach","111111","Brook","Dack","brook_dach@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null),
                    User.CreateNew("kiel_jones","111111","Kiel","Jones","kiel_jones@crnc.com",null,roles.First(r=> r.Title.Equals("Manager")),null)

                };

            return users;
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