using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crnc.Oms.Domain.Aggregates.Users
{
    /// <summary>
    /// Photo of user
    /// </summary>
    public class UserPhoto
    {
        /// <summary>
        /// Id of user
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Photo content
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// Photo content in base64
        /// </summary>
        public string ContentBase64 {
            get
            {
                if(this.Content != null)
                    return Convert.ToBase64String(this.Content);

                return null;
            }
        }

        /// <summary>
        /// Mime type of photo
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// User
        /// </summary>
        public virtual User User { get; set; }

        public UserPhoto()
        {

        }

        public UserPhoto(string base64Content, string mimeType, User user=null)
        {
            if (string.IsNullOrWhiteSpace(base64Content))
                throw new ArgumentNullException("Photo content is empty");
            if (string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentNullException("Photo mimy type content is empty");

            this.Content = Convert.FromBase64String(base64Content);
            this.MimeType = mimeType;
            this.User = user;
        }
    }
}
