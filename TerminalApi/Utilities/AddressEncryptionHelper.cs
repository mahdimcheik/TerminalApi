using TerminalApi.Interfaces;
using TerminalApi.Models;

namespace TerminalApi.Utilities
{
    /// <summary>
    /// Utility class for encrypting and decrypting address data.
    /// Provides helper methods to work with encrypted address information.
    /// </summary>
    public static class AddressEncryptionHelper
    {
        /// <summary>
        /// Encrypts sensitive address fields before saving to database.
        /// </summary>
        /// <param name="address">The address to encrypt</param>
        /// <param name="encryptionService">The encryption service</param>
        /// <returns>Address with encrypted sensitive fields</returns>
        public static Address EncryptAddress(Address address, IEncryptionService encryptionService)
        {
            if (address == null) return address;
            address.Street = !string.IsNullOrEmpty(address.Street) ? encryptionService.Encrypt(address.Street) : address.Street;
            address.StreetLine2 = !string.IsNullOrEmpty(address.StreetLine2) ? encryptionService.Encrypt(address.StreetLine2) : address.StreetLine2;
            return address;
        }

        /// <summary>
        /// Decrypts address fields after retrieving from database.
        /// </summary>
        /// <param name="encryptedAddress">The encrypted address</param>
        /// <param name="encryptionService">The encryption service</param>
        /// <returns>Address with decrypted fields</returns>
        public static Address DecryptAddress(Address encryptedAddress, IEncryptionService encryptionService)
        {
            if (encryptedAddress == null) return encryptedAddress;

            try
            {
                encryptedAddress.Street = !string.IsNullOrEmpty(encryptedAddress.Street) ? encryptionService.Decrypt(encryptedAddress.Street) : encryptedAddress.Street;
                encryptedAddress.StreetLine2 = !string.IsNullOrEmpty(encryptedAddress.StreetLine2) ? encryptionService.Decrypt(encryptedAddress.StreetLine2) : encryptedAddress.StreetLine2;
                return encryptedAddress;
            }
            catch (Exception ex)
            {
                // Log the error and return the original address if decryption fails
                // This handles cases where data might not be encrypted yet during migration
                Console.WriteLine($"Error decrypting address: {ex.Message}");
                return encryptedAddress;
            }
        }

        /// <summary>
        /// Encrypts address fields in a DTO before saving.
        /// </summary>
        /// <param name="addressDto">The address DTO to encrypt</param>
        /// <param name="encryptionService">The encryption service</param>
        /// <returns>DTO with encrypted fields</returns>
        public static AddressCreateDTO EncryptAddressDto(AddressCreateDTO addressDto, IEncryptionService encryptionService)
        {
            if (addressDto == null) return addressDto;
            addressDto.Street = !string.IsNullOrEmpty(addressDto.Street) ? encryptionService.Encrypt(addressDto.Street) : addressDto.Street;
            addressDto.StreetLine2 = !string.IsNullOrEmpty(addressDto.StreetLine2) ? encryptionService.Encrypt(addressDto.StreetLine2) : addressDto.StreetLine2;
            return addressDto;
        }

        /// <summary>
        /// Decrypts address fields in a response DTO.
        /// </summary>
        /// <param name="encryptedDto">The encrypted address DTO</param>
        /// <param name="encryptionService">The encryption service</param>
        /// <returns>DTO with decrypted fields</returns>
        public static AddressResponseDTO DecryptAddressResponseDto(AddressResponseDTO encryptedAddress, IEncryptionService encryptionService)
        {
            if (encryptedAddress == null) return encryptedAddress;

            try
            {
                encryptedAddress.Street = !string.IsNullOrEmpty(encryptedAddress.Street) ? encryptionService.Decrypt(encryptedAddress.Street) : encryptedAddress.Street;
                encryptedAddress.StreetLine2 = !string.IsNullOrEmpty(encryptedAddress.StreetLine2) ? encryptionService.Decrypt(encryptedAddress.StreetLine2) : encryptedAddress.StreetLine2;
                return encryptedAddress;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting address DTO: {ex.Message}");
                return encryptedAddress;
            }
        }
    }
}
