using System.Security.Cryptography;
using System.Text;

public static class AadhaarHelper
{
    public static byte[] GenerateSalt(int size = 16)
    {
        byte[] salt = new byte[size];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }


    public static string ComputeHash(string aadhaar, byte[] salt)
    {
        byte[] aadhaarBytes = Encoding.UTF8.GetBytes(aadhaar);
        byte[] combined = new byte[aadhaarBytes.Length + salt.Length];
        Buffer.BlockCopy(aadhaarBytes, 0, combined, 0, aadhaarBytes.Length);
        Buffer.BlockCopy(salt, 0, combined, aadhaarBytes.Length, salt.Length);

        using (SHA256 sha = SHA256.Create())
        {
            byte[] hashBytes = sha.ComputeHash(combined);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

  
    public static string ComputeQuickHash(string aadhaar)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(aadhaar + "STATIC_KEY"); 
            byte[] hashBytes = sha.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 16).ToLower();
        }
    }
}
