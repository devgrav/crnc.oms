using System;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Crnc.Oms.Infrastructure.CrossCutting
{
    public static class PasswordHelper
    {
        public static (string Hash, string Salt) GetHash(string password)
        {
            var salt = GetSalt();
            using(var hashProvider = SHA256.Create())
            {
                var passwordWithSalt = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
                var hash = hashProvider.ComputeHash(passwordWithSalt);

                return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
            }
        }

        public static bool IsRightPassword(string expectedHash, string salt, string passwordForCompare)
        {
            var passwordWithSalt = Encoding.UTF8.GetBytes(passwordForCompare).Concat(Convert.FromBase64String(salt)).ToArray();

            using(var hashProvider = SHA256.Create())
            {
                var actualHash = hashProvider.ComputeHash(passwordWithSalt);

                return expectedHash.Equals(Convert.ToBase64String(actualHash));
            }                       
        }

        private static byte[] GetSalt()
        {
            using(var provider = new RNGCryptoServiceProvider())
            {
                var data = new byte[2];
                provider.GetNonZeroBytes(data);
                return data;
            }
        }
    }
}