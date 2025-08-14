using Microsoft.AspNetCore.Mvc;
using TerminalApi.Models;
using TerminalApi.Models.Bookings;

namespace TerminalApi.Interfaces
{
    public interface IBookingService
    {
        Task<bool> BookSlot(BookingCreateDTO newBookingCreateDTO, UserApp booker);
        Task<bool> RemoveReservationByTeacher(string slotId);
        Task<bool> RemoveReservationByStudent(string slotId, string studentId);
        Task<List<ChatMessage>> GetCommunicationsForBooking(Guid bookingid);
        Task<bool> AddMessage(Guid bookingId, ChatMessage newMessage);
        Task<(long Count, List<BookingResponseDTO>? Data)> GetTeacherReservations(QueryPagination query);
        Task<(long Count, List<BookingResponseDTO>? Data)> GetStudentReservations(QueryPagination query, UserApp student);
    }
} 