using System;
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
                var hash = hashProvider.ComputeHash(Encoding.UTF8.GetBytes(password));

                return (hash + salt, salt);
            }
        }

        public static bool IsRightPassword(string expectedHash, string salt, string passwordForCompare)
        {
            var passwordWithSalt = passwordForCompare + salt;

            using(var hashProvider = SHA256.Create())
            {
                var actualHash = hashProvider.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));

                return expectedHash.Equals(actualHash);
            }                       
        }

        private static string GetSalt()
        {
            using(var provider = new RNGCryptoServiceProvider())
            {
                var data = new Byte[1];
                provider.GetNonZeroBytes(data);
                var salt = Encoding.UTF8.GetString(data);
                return salt;
            }
        }
    }
}