using TerminalApi.Contexts;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.User;
using TerminalApi.Models;
using Microsoft.EntityFrameworkCore;

namespace TerminalApi.Services
{
    public class BookingService
    {
        private readonly SlotService slotService;
        private readonly ApiDefaultContext context;


        public BookingService(SlotService slotService, ApiDefaultContext context)
        {
            this.slotService = slotService;
            this.context = context;
        }

        public async Task<bool> BookSlot(BookingCreateDTO newBookingCreateDTO, string bookerId)
        {
            var slot = await context
                .Slots.Where(x =>
                    x.Id == Guid.Parse(newBookingCreateDTO.SlotId)
                    && x.StartAt > DateTimeOffset.UtcNow
                )
                .Include(x => x.Booking)
                .FirstOrDefaultAsync();
            if (slot is null || slot.Booking is not null)
            {
                return false;
            }

            Booking newBooking = newBookingCreateDTO.ToBooking(bookerId);
            try
            {
                var res = await context.Bookings.AddAsync(newBooking);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Réservation non créée");
            }
        }
        public async Task<bool> RemoveReservationByTeacher(string slotId)
        {
            var slot = await context
                .Slots.AsSplitQuery()
                .Include(x => x.Booking)
                .Where(x => x.Id == Guid.Parse(slotId))
                .FirstOrDefaultAsync();

            if (slot.Booking is null)
            {
                return false;
            }
            try
            {
                var res = context.Bookings.Remove(slot.Booking);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Résérvation non supprimée ou non existante");
            }
        }
        public async Task<bool> RemoveReservationByStudent(string slotId, string studentId)
        {
            var slot = await context
                .Slots.AsSplitQuery()
                .Include(x => x.Booking)
                .Where(x => x.Id == Guid.Parse(slotId) && x.Booking.BookedById == studentId)
                .FirstOrDefaultAsync();

            if (slot is null || slot.Booking is null)
            {
                return false;
            }
            try
            {
                var res = context.Bookings.Remove(slot.Booking);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Résérvation non supprimée ou non existante");
            }
        }
        public async Task<(long Count, List<BookingResponseDTO>? Data)> GetTeacherReservations(QueryPagination query)
        {
            var sqlQuery = context
                .Bookings.Include(re => re.Slot)
                .Include(re => re.Order)
                .Include(re => re.Booker).Where(x => x != null);

            if (!string.IsNullOrEmpty(query.StudentId))
            {
                sqlQuery = sqlQuery.Where(re => re.BookedById == query.StudentId);
            }

            if (query.FromDate.HasValue)
            {
                sqlQuery = sqlQuery.Where(re => re.Slot.StartAt >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                sqlQuery = sqlQuery.Where(re => re.Slot.EndAt <= query.ToDate.Value);
            }

            var count = await sqlQuery.CountAsync();
            List<BookingResponseDTO>? result = await sqlQuery
                .Skip(query.Start)
                .Take(query.PerPage)
                .Select(re => re.ToBookingResponseDTO())
                .ToListAsync();

            return (count, result);
        }
        public async Task<(long Count, List<BookingResponseDTO>? Data)> GetStudentReservations(QueryPagination query, UserApp student)
        {
            var sqlQuery = context
                .Bookings.Include(re => re.Slot)
                .Include(re => re.Order)
                .Where(x => x != null && x.BookedById == student.Id);

            if (query.FromDate.HasValue)
            {
                sqlQuery = sqlQuery.Where(re => re.Slot.StartAt >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                sqlQuery = sqlQuery.Where(re => re.Slot.EndAt <= query.ToDate.Value);
            }
            if (query.OrderByDate is not null && query.OrderByDate == 1)
            {
                sqlQuery = sqlQuery.OrderBy(x => x.Slot.StartAt);
            }
            if (query.OrderByDate is not null && query.OrderByDate == -1)
            {
                sqlQuery = sqlQuery.OrderBy(x => x.Slot.EndAt);
            }
            if (query.OrderByName is not null && query.OrderByName == 1)
            {
                sqlQuery = sqlQuery.OrderBy(x => x.Booker.LastName).ThenBy(x => x.Booker.FirstName);
            }
            if (query.OrderByName is not null && query.OrderByName == -1)
            {
                sqlQuery = sqlQuery.OrderByDescending(x => x.Booker.LastName).ThenByDescending(x => x.Booker.FirstName);
            }

            var count = await sqlQuery.CountAsync();

            var toto = sqlQuery
                .AsSplitQuery()
                .Skip(query.Start)
                .Take(query.PerPage)
                .Select(re => re.ToBookingResponseDTO()).ToQueryString();

            List<BookingResponseDTO>? result = await sqlQuery
                .AsSplitQuery()
                .AsNoTracking()
                .Skip(query.Start)
                .Take(query.PerPage)
                .Select(re => re.ToBookingResponseDTO(student))
                .ToListAsync();

            return (count, result);
        }
    }
}
