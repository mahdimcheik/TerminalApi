using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models.Adresse;
using TerminalApi.Models.User;

namespace TerminalApi.Services
{
    public class AddressService
    {
        private readonly UserManager<UserApp> userManager;
        private readonly ApiDefaultContext context;

        public AddressService(UserManager<UserApp> userManager, ApiDefaultContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }

        public async Task<List<AddressResponseDTO>?> GetAddresses(string userId)
        {
            return await context
                .Addresses.Where(ad => ad.UserId == userId)
                .Select(ad => ad.ToAddressDTO())
                .ToListAsync();
        }

        public async Task<AddressResponseDTO> AddAddress(AddressCreateDTO addressCreate, string userId)
        {
            try
            {
                var listAdresses = await context.Addresses.Where(x => x.UserId == userId).ToListAsync();
                if(listAdresses.Count >= 5)
                {
                    throw new Exception("Nombre maximale d'adresses atteint");
                }
                var adress = addressCreate.ToAddress(userId);
                context.Addresses.Add(adress);
                await context.SaveChangesAsync();
                return adress.ToAddressDTO();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<AddressResponseDTO> UpdateAddress(AddressUpdateDTO updatedAddressData, Address address)
        {
            try
            {
                updatedAddressData.ToAddress(address);
                await context.SaveChangesAsync();
                return address.ToAddressDTO();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> DeleteAddress(string userId, string addressId)
        {
            try
            {
                var address = await context.Addresses.FirstOrDefaultAsync(x => x.Id == Guid.Parse(addressId) && x.UserId == userId);
                if (address is null) throw new Exception("L'adresse n'existe pas");
                context.Addresses.Remove(address);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
