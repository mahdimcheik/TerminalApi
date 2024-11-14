namespace TerminalApi.Models.Adresse
{
    public class AddressResponseDTO
    {
        public string Id { get; set; }
        public int StreetNumber { get; set; }
        public string Street { get; set; }

        public string? StreetLine2 { get; set; }

        public string City { get; set; }

        public string? State { get; set; }

        public string? PostalCode { get; set; }

        public string? Country { get; set; }

        public AddressTypeEnum? AddressType { get; set; }
        public string? UserId { get; set; }
    }
}
