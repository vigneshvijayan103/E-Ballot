namespace EBallotApi.Helper
{
    public static class SafeDecryptHelper
    {
        public static string SafeDecrypt(string encryptedValue)
        {
            try
            {
                return !string.IsNullOrEmpty(encryptedValue)
                    ? AesEncryptionHelper.Decrypt(encryptedValue)
                    : string.Empty;
            }
            catch
            {
                return "[Decryption Error]";
            }
        }

        public static string SafeDecryptAndMaskAadhaar(string aadhaarEnc)
        {
            try
            {
                if (!string.IsNullOrEmpty(aadhaarEnc))
                {
                    var decrypted = AesEncryptionHelper.Decrypt(aadhaarEnc);
                    return AesEncryptionHelper.MaskAadhaar(decrypted);
                }
                return string.Empty;
            }
            catch
            {
                return "[Decryption Error]";
            }
        }
    }
}
