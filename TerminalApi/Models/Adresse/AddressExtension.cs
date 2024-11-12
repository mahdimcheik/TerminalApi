using System.Runtime.CompilerServices;

namespace TerminalApi.Models.Adresse
{
    public static  class AddressExtension
    {
        public static AddressResponseDTO ToAddressDTO (this Address adress)
        {
            return new AddressResponseDTO
            {
                Id = adress.Id.ToString(),
                City = adress.City,
                State = adress.State,
                PostalCode = adress.PostalCode,
                Country = adress.Country,
                Street = adress.Street,
                StreetLine2 = adress.StreetLine2,
                StreetNumber = adress.StreetNumber,
                AddressType = adress.AddressType,
                UserId = adress.UserId
            };
        }

        public static Address ToAddress(this AddressCreateDTO adress)
        {
            return new Address
            {
                City = adress.City,
                State = adress.State ?? "",
                PostalCode = adress.PostalCode,
                Country = adress.Country,
                Street = adress.Street,
                StreetLine2 = adress.StreetLine2 ?? "",
                StreetNumber = adress.StreetNumber,
                AddressType = adress.AddressType,
                UserId = adress.UserId
            };
        }

        
    }
}
