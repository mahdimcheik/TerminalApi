using System.Security.Cryptography;
using System.Text;
using TerminalApi.Interfaces;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    /// <summary>
    /// Service for encrypting and decrypting sensitive data using AES encryption.
    /// Used primarily for protecting client addresses and other personal information.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly string _encryptionKey;

        public EncryptionService()
        {
            _encryptionKey = EnvironmentVariables.ENCRYPTION_KEY;
            
            if (string.IsNullOrEmpty(_encryptionKey))
            {
                throw new InvalidOperationException("Encryption key is not configured. Please set ENCRYPTION_KEY environment variable.");
            }

            if (_encryptionKey.Length < 32)
            {
                throw new InvalidOperationException("Encryption key must be at least 32 characters long for AES-256.");
            }
        }

        /// <summary>
        /// Encrypts a plain text string using AES-256 encryption.
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <returns>Base64 encoded encrypted string</returns>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                using (var aes = Aes.Create())
                {
                    // Use the first 32 bytes of the encryption key for AES-256
                    aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.Substring(0, 32));
                    aes.GenerateIV();
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var msEncrypt = new MemoryStream())
                    {
                        // Prepend IV to the encrypted data
                        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error encrypting data: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Decrypts an encrypted string back to plain text.
        /// </summary>
        /// <param name="encryptedText">Base64 encoded encrypted string</param>
        /// <returns>Decrypted plain text</returns>
        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                var fullCipher = Convert.FromBase64String(encryptedText);

                using (var aes = Aes.Create())
                {
                    // Use the first 32 bytes of the encryption key for AES-256
                    aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.Substring(0, 32));
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    // Extract IV from the beginning of the encrypted data
                    var iv = new byte[aes.BlockSize / 8];
                    var cipher = new byte[fullCipher.Length - iv.Length];

                    Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                    Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var msDecrypt = new MemoryStream(cipher))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error decrypting data: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates if an encrypted string can be successfully decrypted.
        /// </summary>
        /// <param name="encryptedText">The encrypted text to validate</param>
        /// <returns>True if the text can be decrypted, false otherwise</returns>
        public bool IsValidEncryptedData(string encryptedText)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedText))
                    return false;

                var decrypted = Decrypt(encryptedText);
                return !string.IsNullOrEmpty(decrypted);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a new encryption key suitable for AES-256 encryption.
        /// This method is useful for generating new keys for different environments.
        /// </summary>
        /// <returns>A new 32-character encryption key</returns>
        public static string GenerateEncryptionKey()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var keyBytes = new byte[32]; // 256 bits
                rng.GetBytes(keyBytes);
                return Convert.ToBase64String(keyBytes);
            }
        }
    }
}
