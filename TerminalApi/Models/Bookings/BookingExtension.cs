﻿namespace TerminalApi.Models.Bookings
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

        public static BookingResponseDTO ToBookingResponseDTO(this Booking booking)
        {
            return new BookingResponseDTO
            {
                Id = booking.Id,
                Subject = booking.Subject,
                Description = booking.Description,
                TypeHelp = booking.TypeHelp,
                OrderId = booking.OrderId,
                CreatedAt = booking.CreatedAt,
                Price = booking.Slot.Price,
                DiscountedPrice = booking.Slot.DiscountedPrice,
                Reduction = booking.Slot.Reduction,
                SlotId = booking.SlotId,
                StartAt = booking.Slot.StartAt,
                EndAt = booking.Slot.EndAt,
                Type = booking.Slot.Type,
                StudentId = booking.BookedById,
                StudentFirstName = booking.Booker.FirstName,
                StudentLastName = booking.Booker.LastName,
                StudentImgUrl = booking.Booker.ImgUrl
            };
        }
    }
}
