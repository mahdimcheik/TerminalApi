using TerminalApi.Models;

namespace TerminalApi.Interfaces
{
    public interface IBookingService
    {
        Task<bool> BookSlot(BookingCreateDTO newBookingCreateDTO, UserApp booker);
        Task<bool> RemoveReservationByTeacher(string slotId);
        Task<bool> RemoveReservationByStudent(string slotId, string studentId);
        Task<(long Count, List<BookingResponseDTO>? Data)> GetTeacherReservations(QueryPagination query);
        Task<(long Count, List<BookingResponseDTO>? Data)> GetStudentReservations(QueryPagination query, UserApp student);
    }
} 