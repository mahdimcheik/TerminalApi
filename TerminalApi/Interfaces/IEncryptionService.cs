namespace TerminalApi.Interfaces
{
    /// <summary>
    /// Interface for encryption and decryption services.
    /// Provides methods to encrypt and decrypt sensitive data like client addresses.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts a plain text string.
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <returns>Encrypted string encoded in Base64</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypts an encrypted string back to plain text.
        /// </summary>
        /// <param name="encryptedText">Base64 encoded encrypted string</param>
        /// <returns>Decrypted plain text</returns>
        string Decrypt(string encryptedText);

        /// <summary>
        /// Validates if an encrypted string can be successfully decrypted.
        /// </summary>
        /// <param name="encryptedText">The encrypted text to validate</param>
        /// <returns>True if the text can be decrypted, false otherwise</returns>
        bool IsValidEncryptedData(string encryptedText);
    }
}
