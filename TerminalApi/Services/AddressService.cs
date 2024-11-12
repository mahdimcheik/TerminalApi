using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models.Adresse;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

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

        public async Task<AddressResponseDTO> AddAddress(AddressCreateDTO addressCreate)
        {
            try
            {
                var result = await CheckUser.CheckUserNullByUserId(
                    addressCreate.UserId,
                    userManager
                );
                if (result)
                {
                    throw new Exception("l'utilisateur n'existe pas");
                }

                var adress = addressCreate.ToAddress();
                context.Addresses.Add(adress);
                await context.SaveChangesAsync();
                return adress.ToAddressDTO();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<AddressResponseDTO> UpdateAddress(Address updatedAddressData)
        {
            try
            {
                context.Addresses.Attach(updatedAddressData);
                context.Entry(updatedAddressData).State = EntityState.Modified;
                await context.SaveChangesAsync();
                var res = await context.Addresses.FirstOrDefaultAsync(x =>
                    x.Id == updatedAddressData.Id
                );
                if (res is not null)
                    return res.ToAddressDTO();
                else
                    throw new Exception("Il n'y a pas d'addresse à modifier");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
