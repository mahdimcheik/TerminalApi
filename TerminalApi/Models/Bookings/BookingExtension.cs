namespace TerminalApi.Models.Bookings
{
    public static class BookingExtension
    {
        public static Booking ToBooking(this BookingCreateDTO bookingCreateDTO, string userId)
        {
            return new Booking()
            {
                SlotId = Guid.Parse(bookingCreateDTO.SlotId),
                BookedById = userId,
                CreatedAt = DateTimeOffset.Now,
                Subject = bookingCreateDTO.Subject,
                Description = bookingCreateDTO.Description,
                TypeHelp = bookingCreateDTO.TypeHelp ?? 0
            };
        }
    }
}
