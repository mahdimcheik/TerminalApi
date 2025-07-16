using TerminalApi.Models;

namespace TerminalApi.Interfaces
{
    public interface IAddressService
    {
        Task<List<AddressResponseDTO>?> GetAddresses(string userId);
        Task<AddressResponseDTO> AddAddress(AddressCreateDTO addressCreate, string userId);
        Task<AddressResponseDTO> UpdateAddress(AddressUpdateDTO updatedAddressData, Address address);
        Task<bool> DeleteAddress(string userId, string addressId);
    }
} 