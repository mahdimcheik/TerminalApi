namespace TerminalApi.Models
{
    public static class AddressExtension
    {
        public static AddressResponseDTO ToAddressDTO(this Address adress)
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
                UserId = adress.UserId,
            };
        }

        public static Address ToAddress(this AddressCreateDTO adress, string userId)
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
                UserId = userId,
            };
        }

        public static Address ToAddress(this AddressUpdateDTO adress, Address old)
        {
            old.City = adress.City;
            old.State = adress.State;
            old.PostalCode = adress.PostalCode;
            old.Country = adress.Country;
            old.Street = adress.Street;
            old.StreetLine2 = adress.StreetLine2;
            old.StreetNumber = adress.StreetNumber;
            old.AddressType = adress.AddressType;
            old.Id = Guid.Parse(adress.Id);
            return old;
        }
    }
}
