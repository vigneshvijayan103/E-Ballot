using System;
using System.Security.Cryptography;
using System.Text;

namespace EBallotApi.Helper
{
    public class AesEncryptionHelper
    {
        private static readonly byte[] Key;
        private static readonly byte[] IV;

        static AesEncryptionHelper()
        {
            string key = Environment.GetEnvironmentVariable("AES_KEY");
            string iv = Environment.GetEnvironmentVariable("AES_IV");



            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv))
                throw new Exception("AES Key or IV not configured in environment variables.");

            if (key.Length != 32)
                throw new Exception($"AES Key must be 32 chars. Current length: {key.Length}");
            if (iv.Length != 16)
                throw new Exception($"AES IV must be 16 chars. Current length: {iv.Length}");

            Key = Encoding.UTF8.GetBytes(key);
            IV = Encoding.UTF8.GetBytes(iv);

            Console.WriteLine("✅ AES helper initialized successfully");
        }

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(cipherBytes);
        }

        public static string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var decryptor = aes.CreateDecryptor();
            var cipherBytes = Convert.FromBase64String(cipherText);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
