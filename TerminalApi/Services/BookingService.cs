using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Interfaces;
using TerminalApi.Models;
using TerminalApi.Models.Bookings;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApiDefaultContext context;
        private readonly IOrderService orderService;
        private readonly INotificationService notificationService;
        private readonly IJobChron jobChron;
        private readonly ISignalRNotificationService signalRNotificationService;

        public BookingService(
            ApiDefaultContext context,
            IOrderService orderService,
            INotificationService notificationService,
            IJobChron jobChron,
            ISignalRNotificationService signalRNotificationService
        )
        {
            this.context = context;
            this.orderService = orderService;
            this.notificationService = notificationService;
            this.jobChron = jobChron;
            this.signalRNotificationService = signalRNotificationService;
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

            Order order = await orderService.GetOrCreateCurrentOrderByUserAsync(booker);

            if (slot is null || slot.Booking is not null || order is null)
            {
                return false;
            }

            if (order.CheckoutID is not null)
            {
                try
                {
                    await jobChron.ExpireCheckout(order.CheckoutID);
                    order.ResetCheckout();
                }
                catch { }
            }

            Booking newBooking = newBookingCreateDTO.ToBooking(booker.Id, order.Id);
            try
            {
                var res = await context.Bookings.AddAsync(newBooking);
                //await context.SaveChangesAsync();
                //var notification = new Notification
                //{
                //    RecipientId = booker.Id,
                //    Type = Utilities.EnumNotificationType.ReservationAccepted
                //};
                //var notificationForTeacher = new Notification
                //{
                //    RecipientId = EnvironmentVariables.TEACHER_ID,
                //    SenderId = booker.Id,
                //    Type = Utilities.EnumNotificationType.NewReservation
                //};
                //await notificationService.AddNotification(notificationForTeacher);
                //var notificationDb = await notificationService.AddNotification(notification);
                await orderService.UpdateOrderAsync(booker, order.Id);

                jobChron.SchedulerSingleOrderCleaning(order.Id.ToString());

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
                var orderId = slot.Booking.OrderId;
                var res = context.Bookings.Remove(slot.Booking);

                var order = context
                    .Orders.Where(x => x.Id == orderId)
                    .Include(x => x.Bookings)
                    .FirstOrDefault();

                if (order is not null && !order.CheckoutID.IsNullOrEmpty())
                {
                    await jobChron.ExpireCheckout(order.CheckoutID);
                    order.ResetCheckout();
                }

                var affectedLines = await context.SaveChangesAsync();

                if (affectedLines != 0)
                {
                    if (order.Bookings is not null && order.Bookings.Count == 0)
                    {
                        jobChron.CancelScheuledJob(order.Id.ToString());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Résérvation non supprimée ou non existante");
            }
        }

        public async Task<(long Count, List<BookingResponseDTO>? Data)> GetTeacherReservations(
            QueryPagination query
        )
        {
            var sqlQuery = context
                .Bookings.Include(re => re.Slot)
                .Include(re => re.Order)
                .Include(re => re.Booker)
                .Where(x => x != null);

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

            if (!string.IsNullOrEmpty(query.SearchWord))
            {
                var searchTerm = query.SearchWord.Trim().ToLower();
                sqlQuery = sqlQuery.Where(re =>
                    // Search in student names
                    EF.Functions.ILike(re.Booker.Email, $"%{searchTerm}%") ||
                    EF.Functions.ILike(string.Concat(re.Booker.FirstName.ToLower(), " ", re.Booker.LastName.ToLower()), $"%{searchTerm}%") ||
                    // Search in booking details
                    EF.Functions.ILike(re.Subject ?? "", $"%{searchTerm}%") ||
                    EF.Functions.ILike(re.Description ?? "", $"%{searchTerm}%") ||
                    // Keep ID search for power users
                    re.Id.ToString() == searchTerm ||
                    EF.Functions.ILike(re.Order.OrderNumber.ToLower(), $"%{searchTerm}%")
                );
            }

            var count = await sqlQuery.CountAsync();
            List<BookingResponseDTO>? result = await sqlQuery
                .Skip(query.Start)
                .Take(query.PerPage)
                .Select(re => re.ToBookingResponseDTO())
                .ToListAsync();

            return (count, result);
        }

        public async Task<(long Count, List<BookingResponseDTO>? Data)> GetStudentReservations(
            QueryPagination query,
            UserApp student
        )
        {
            var sqlQuery = context
                .Bookings.Include(re => re.Slot)
                .Include(re => re.Order)
                .Include(re => re.Booker)
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
                sqlQuery = sqlQuery
                    .OrderByDescending(x => x.Booker.LastName)
                    .ThenByDescending(x => x.Booker.FirstName);
            }


            if (!string.IsNullOrEmpty(query.SearchWord))
            {
                var searchTerm = query.SearchWord.Trim().ToLower();
                sqlQuery = sqlQuery.Where(re =>
                    // Search in student names
                    EF.Functions.ILike(re.Booker.Email, $"%{searchTerm}%") ||
                    EF.Functions.ILike(string.Concat(re.Booker.FirstName.ToLower(), " ", re.Booker.LastName.ToLower()), $"%{searchTerm}%") ||
                    // Search in booking details
                    EF.Functions.ILike(re.Subject ?? "", $"%{searchTerm}%") ||
                    EF.Functions.ILike(re.Description ?? "", $"%{searchTerm}%") ||
                    // Keep ID search for power users
                    re.Id.ToString() == searchTerm ||
                    EF.Functions.ILike(re.Order.OrderNumber.ToLower(), $"%{searchTerm}%")
                );
            }


            var count = await sqlQuery.CountAsync();

            List<BookingResponseDTO>? result = await sqlQuery
                .AsSplitQuery()
                .AsNoTracking()
                .Skip(query.Start)
                .Take(query.PerPage)
                .Select(re => re.ToBookingResponseDTO(student))
                .ToListAsync();

            return (count, result);
        }

        public async Task<List<ChatMessage>> GetCommunicationsForBooking(Guid bookingid)
        {
            var sqlQuery = await context
                .Bookings.Where(x => x.Id == bookingid)
                .Select(x => x.Communications)
                .FirstOrDefaultAsync();
            return sqlQuery?.ToList() ?? [];
        }

        public async Task<bool> AddMessage(Guid bookingId, ChatMessage newMessage)
        {
            var booking = await context.Bookings.Include(x => x.Booker).FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking is null)
            {
                return false;
            }

            try
            {
                if (booking.Communications is null)
                {
                    booking.Communications = new List<ChatMessage>();
                }

                booking.Communications.Add(newMessage);
                context.Entry(booking).State = EntityState.Modified;
                await context.SaveChangesAsync();
                await signalRNotificationService.SendMessageByUserEmail(booking.Booker.Email, MessageTypeEnum.Chat, newMessage.Message);
                await signalRNotificationService.SendMessageByUserEmail(EnvironmentVariables.TEACHER_EMAIL, MessageTypeEnum.Chat, newMessage.Message);
                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}
