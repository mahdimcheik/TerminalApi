using TerminalApi.Contexts;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.User;
using TerminalApi.Models;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Models.Payments;
using TerminalApi.Models.Notification;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class BookingService
    {
        private readonly ApiDefaultContext context;
        private readonly OrderService orderService;
        private readonly SseService sseService;
        private readonly NotificationService notificationService;
        private readonly JobChron jobChron;

        public BookingService( ApiDefaultContext context, OrderService orderService, SseService sseService, NotificationService notificationService, JobChron jobChron)
        {
            this.context = context;
            this.orderService = orderService;
            this.sseService = sseService;
            this.notificationService = notificationService;
            this.jobChron = jobChron;
        }

        public async Task<bool> BookSlot(BookingCreateDTO newBookingCreateDTO, UserApp booker)
        {
            var slot = await context
                .Slots.Where(x =>
                    x.Id == Guid.Parse(newBookingCreateDTO.SlotId)
                    && x.StartAt > DateTimeOffset.UtcNow
                )
                .Include(x => x.Booking)
                .FirstOrDefaultAsync();

            OrderResponseForStudentDTO orderDTO = await orderService.GetOrCreateCurrentOrderByUserAsync(booker);

            if (slot is null || slot.Booking is not null || orderDTO is null)
            {
                return false;
            }

            if(orderDTO.Status == Utilities.EnumBookingStatus.WaitingForPayment && orderDTO.CheckoutID is not null)
            {
                try
                {
                    await jobChron.ExpireCheckout(orderDTO.CheckoutID);
                }
                catch (Exception ex)
                {
                    throw new Exception("Réservation non créée");
                }
            }

            Booking newBooking = newBookingCreateDTO.ToBooking(booker.Id, orderDTO.Id);
            try
            {
                var res = await context.Bookings.AddAsync(newBooking);
                await context.SaveChangesAsync();
                var notification = new Notification
                {
                    RecipientId = booker.Id,
                    Type = Utilities.EnumNotificationType.ReservationAccepted
                };
                var notificationForTeacher = new Notification
                {
                    RecipientId = EnvironmentVariables.TEACHER_ID,
                    SenderId = booker.Id,
                    Type = Utilities.EnumNotificationType.NewReservation
                };
                await notificationService.AddNotification(notificationForTeacher);
                var notificationDb =   await notificationService.AddNotification(notification);
                await orderService.UpdateOrderAsync(booker, orderDTO.Id);

                jobChron.SchedulerSingleOrderCleaning(orderDTO.Id.ToString());

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
