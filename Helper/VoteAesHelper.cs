using System.Security.Cryptography;
using System.Text;

namespace EBallotApi.Helper
{
    public class VoteAesHelper
    {
        private static readonly byte[] Key;

        static VoteAesHelper()
        {
            string key = Environment.GetEnvironmentVariable("AES_KEY");

            if (string.IsNullOrEmpty(key))
                throw new Exception("AES_KEY not configured in environment variables.");

            if (key.Length != 32)
                throw new Exception($"AES Key must be 32 chars. Current length: {key.Length}");

            Key = Encoding.UTF8.GetBytes(key);
        }

       
        public static (byte[] CipherText, byte[] IV) EncryptVote(string vote)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV(); 

            var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(vote);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return (cipherBytes, aes.IV);
        }

       
        public static string DecryptVote(byte[] cipherText, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}

