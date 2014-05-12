using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EmailProcessor
{
    //http://stackoverflow.com/questions/165808/simple-2-way-encryption-for-c-sharp
    public class SimpleAES
    {
        private static byte[] key = { 151, 58, 71, 118, 254, 145, 231, 28, 216, 185, 6, 243, 77, 53, 30, 131, 184, 185, 20, 232, 145, 47, 96, 135, 10, 230, 116, 142, 158, 197, 101, 223 };
        private static byte[] vector = { 107, 188, 5, 3, 216, 207, 104, 46, 234, 96, 39, 92, 35, 131, 96, 251 };
        private ICryptoTransform encryptor, decryptor;
        private UTF8Encoding encoder;

        public SimpleAES()
        {
            RijndaelManaged rm = new RijndaelManaged();
            rm.Key = key;
            rm.IV = vector;
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
            MemoryStream stream = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                cs.Write(buffer, 0, buffer.Length);
            }
            return stream.ToArray();
        }
    }
}