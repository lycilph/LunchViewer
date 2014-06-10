using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LunchViewerService.Utils
{
    //http://stackoverflow.com/questions/165808/simple-2-way-encryption-for-c-sharp
    public class SimpleAES
    {
        private static readonly byte[] key = { 151, 58, 71, 118, 254, 145, 231, 28, 216, 185, 6, 243, 77, 53, 30, 131, 184, 185, 20, 232, 145, 47, 96, 135, 10, 230, 116, 142, 158, 197, 101, 223 };
        private static readonly byte[] vector = { 107, 188, 5, 3, 216, 207, 104, 46, 234, 96, 39, 92, 35, 131, 96, 251 };
        private readonly ICryptoTransform encryptor;
        private readonly ICryptoTransform decryptor;
        private readonly UTF8Encoding encoder;

        public SimpleAES()
        {
            var rm = new RijndaelManaged { Key = key, IV = vector };
            encryptor = rm.CreateEncryptor();
            decryptor = rm.CreateDecryptor();
            encoder = new UTF8Encoding();
        }

        public string Encrypt(string unencrypted)
        {
            return Convert.ToBase64String(Encrypt(encoder.GetBytes(unencrypted)));
        }

        public string Decrypt(string encrypted)
        {
            return encoder.GetString(Decrypt(Convert.FromBase64String(encrypted)));
        }

        public byte[] Encrypt(byte[] buffer)
        {
            return Transform(buffer, encryptor);
        }

        public byte[] Decrypt(byte[] buffer)
        {
            return Transform(buffer, decryptor);
        }

        protected byte[] Transform(byte[] buffer, ICryptoTransform transform)
        {
            var stream = new MemoryStream();
            using (var cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                cs.Write(buffer, 0, buffer.Length);
            }
            return stream.ToArray();
        }
    }
}