using System.Security.Cryptography;
using System.Text;

namespace MailWorkerRole.Utils
{
    public static class StringExtensions
    {
        public static string ComputeMD5Hash(this string input)
        {
            var provider = new MD5CryptoServiceProvider();
            var hash = provider.ComputeHash(Encoding.Default.GetBytes(input));
            var str = new StringBuilder();
            foreach (var t in hash)
                str.Append(t.ToString("x2"));
            return str.ToString();
        }
    }
}
