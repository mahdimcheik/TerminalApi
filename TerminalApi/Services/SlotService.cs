using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.Slots;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class SlotService
    {
        private readonly ApiDefaultContext context;

        public SlotService(ApiDefaultContext context)
        {
            this.context = context;
        }

        public async Task<SlotResponseDTO> AddSlot(SlotCreateDTO slotCreateDTO, string userId)
        {
            try
            {
                var slot = slotCreateDTO.ToSlot(userId);
                context.Slots.Add(slot);
                await context.SaveChangesAsync();
                return slot.ToResponseDTO();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<SlotResponseDTO> UpdateSlot(SlotUpdateDTO slotUpdateDTO, string userId)
        {
            try
            {
                //var slot = slotCreateDTO.ToSlot(userId);
                var slot = await context.Slots.FirstOrDefaultAsync(x =>
                    x.Id == Guid.Parse(slotUpdateDTO.Id) && x.CreatedById == userId
                );
                if (slot is null)
                    throw new Exception("Le créneau n'existe pas");

                slot.StartAt = slotUpdateDTO.StartAt;
                slot.EndAt = slotUpdateDTO.EndAt;
                slot.Reduction = slotUpdateDTO.Reduction;
                slot.Price = slotUpdateDTO.Price;
                slot.Type = slotUpdateDTO.Type;

                //context.Slots.Add(slot);
                await context.SaveChangesAsync();
                return slot.ToResponseDTO();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<SlotResponseDTO?> GetSlotsById(
            string slotId
        )
        {
            var slot = context
                .Slots.Include(x => x.Booking)
                .ThenInclude(y => y.Booker)
                .AsSplitQuery()
                .FirstOrDefault(ad =>
                    ad.Id == Guid.Parse(slotId)
                );

            if (slot is not null) return slot.ToResponseDTO();
            return null;
        }

        public async Task<List<SlotResponseDTO>?> GetSlotsByCreator(
            string teacherId,
            DateTimeOffset fromDate,
            DateTimeOffset toDate
        )
        {
            return await context
                .Slots.Include(x => x.Booking)
                .ThenInclude(y => y.Booker)
                .AsSplitQuery()
                .Where(ad =>
                    ad.CreatedById == teacherId && ad.StartAt >= fromDate && ad.EndAt <= toDate
                )
                .Select(ad => ad.ToResponseDTO())
                //.Select(ad => new SlotResponseDTO
                //{
                //    Id = ad.Id,
                //    StartAt = ad.StartAt,
                //    EndAt = ad.EndAt,
                //    Price = ad.Price,
                //    DiscountedPrice = ad.DiscountedPrice,
                //    Reduction = ad.Reduction,
                //    StudentFirstName = ad.Booking.Booker.FirstName,
                //    StudentLastName = ad.Booking.Booker.LastName,
                //    StudentImgUrl = ad.Booking.Booker.ImgUrl,
                //    StudentId = ad.Booking.BookedById
                //})
                .ToListAsync();
        }

        public async Task<List<SlotResponseDTO>?> GetSlotsByStudent(
            string userId,
            DateTimeOffset fromDate,
            DateTimeOffset toDate
        )
        {
            return await context
                .Slots.Include(x => x.Booking)
                .ThenInclude(y => y.Booker)
                .AsSplitQuery()
                .Where(ad =>
                    (
                        ad.CreatedById == EnvironmentVariables.TEACHER_ID
                        && ad.StartAt >= fromDate
                        && ad.EndAt <= toDate
                        && ad.StartAt >= DateTimeOffset.UtcNow
                        && ad.Booking == null
                    )
                    || (
                        ad.CreatedById == EnvironmentVariables.TEACHER_ID
                        && ad.StartAt >= fromDate
                        && ad.EndAt <= toDate
                        && ad.Booking != null
                        && ad.Booking.BookedById == userId
                    )
                )
                .Select(ad => ad.ToResponseDTO())
                //new SlotResponseDTO()
                //{
                //    Id = ad.Id,
                //    StartAt = ad.StartAt,
                //    EndAt = ad.EndAt,
                //    Price = ad.Price,
                //    DiscountedPrice = ad.DiscountedPrice,
                //    Reduction = ad.Reduction,
                //    StudentFirstName = ad.Booking.Booker.FirstName ,
                //    StudentLastName = ad.Booking.Booker.LastName,
                //    StudentImgUrl = ad.Booking.Booker.ImgUrl,
                //    StudentId = ad.Booking.BookedById,
                //    Subject = ad.Booking.Subject,
                //    Description = ad.Booking.Description,
                //    TypeHelp = ad.Booking.TypeHelp
                //})
                .ToListAsync();
        }

        public async Task<List<SlotResponseDTO>?> GetSlotsForStudent(
            string studentid,
            string teacherId,
            DateTimeOffset fromDate,
            DateTimeOffset toDate
        )
        {
            return await context
                .Slots.Where(ad =>
                    ad.CreatedById == teacherId && ad.StartAt >= fromDate && ad.EndAt <= toDate
                )
                .Select(ad => ad.ToResponseDTO())
                .ToListAsync();
        }

        public async Task<bool> DeleteSlot(string userId, string slotId)
        {
            try
            {
                var slot = await context
                    .Slots.Include(x => x.Booking)
                    .FirstOrDefaultAsync(x =>
                        x.Id == Guid.Parse(slotId) && x.CreatedById == userId
                    );
                if (slot is null)
                    throw new Exception("Le créneau n'existe pas");
                if (slot.Booking is not null)
                    throw new Exception("Le créneau est réservé, rafraichir ?");
                if (slot.StartAt < DateTimeOffset.UtcNow)
                    throw new Exception("Le créneau ne peut être supprimé");
                context.Slots.Remove(slot);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //public async Task<bool> BookSlot(BookingCreateDTO newBookingCreateDTO, string bookerId)
        //{
        //    var slot = await context
        //        .Slots.Where(x =>
        //            x.Id == Guid.Parse(newBookingCreateDTO.SlotId)
        //            && x.StartAt > DateTimeOffset.UtcNow
        //        )
        //        .Include(x => x.Booking)
        //        .FirstOrDefaultAsync();
        //    if (slot is null || slot.Booking is not null)
        //    {
        //        return false;
        //    }

        //    Booking newBooking = newBookingCreateDTO.ToBooking(bookerId);
        //    try
        //    {
        //        var res = await context.Bookings.AddAsync(newBooking);
        //        await context.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Résérvation non créée");
        //    }
        //}

        //public async Task<bool> RemoveReservationByTeacher(string slotId)
        //{
        //    var slot = await context
        //        .Slots.AsSplitQuery()
        //        .Include(x => x.Booking)
        //        .Where(x => x.Id == Guid.Parse(slotId))
        //        .FirstOrDefaultAsync();

        //    if (slot.Booking is null)
        //    {
        //        return false;
        //    }
        //    try
        //    {
        //        var res = context.Bookings.Remove(slot.Booking);
        //        await context.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Résérvation non supprimée ou non existante");
        //    }
        //}

        //public async Task<bool> RemoveReservationByStudent(string slotId, string studentId)
        //{
        //    var slot = await context
        //        .Slots.AsSplitQuery()
        //        .Include(x => x.Booking)
        //        .Where(x => x.Id == Guid.Parse(slotId) && x.Booking.BookedById == studentId)
        //        .FirstOrDefaultAsync();

        //    if (slot is null || slot.Booking is null)
        //    {
        //        return false;
        //    }
        //    try
        //    {
        //        var res = context.Bookings.Remove(slot.Booking);
        //        await context.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Résérvation non supprimée ou non existante");
        //    }
        //}

        //public async Task<(long Count, List<BookingResponseDTO>? Data)> GetTeacherReservations(QueryPagination query)
        //{
        //    var sqlQuery = context
        //        .Bookings.Include(re => re.Slot)
        //        .Include(re => re.Order)
        //        .Include(re => re.Booker).Where(x => x != null);

        //    if (!string.IsNullOrEmpty(query.StudentId))
        //    {
        //        sqlQuery = sqlQuery.Where(re => re.BookedById == query.StudentId);
        //    }

        //    if (query.FromDate.HasValue)
        //    {
        //        sqlQuery = sqlQuery.Where(re => re.Slot.StartAt >= query.FromDate.Value);
        //    }

        //    if (query.ToDate.HasValue)
        //    {
        //        sqlQuery = sqlQuery.Where(re => re.Slot.EndAt <= query.ToDate.Value);
        //    }

        //    var count = await sqlQuery.CountAsync();
        //    List<BookingResponseDTO>? result = await sqlQuery
        //        .Skip(query.Start)
        //        .Take(query.PerPage)
        //        .Select(re => re.ToBookingResponseDTO())
        //        .ToListAsync();

        //    return (count, result);
        //}

        //public async Task<(long Count, List<BookingResponseDTO>? Data)> GetStudentReservations(QueryPagination query, UserApp student)
        //{
        //    var sqlQuery = context
        //        .Bookings.Include(re => re.Slot)
        //        .Include(re => re.Order)
        //        .Where(x => x != null && x.BookedById == student.Id);

        //    if (query.FromDate.HasValue)
        //    {
        //        sqlQuery = sqlQuery.Where(re => re.Slot.StartAt >= query.FromDate.Value);
        //    }

        //    if (query.ToDate.HasValue)
        //    {
        //        sqlQuery = sqlQuery.Where(re => re.Slot.EndAt <= query.ToDate.Value);
        //    }
        //    if(query.OrderByDate is not null && query.OrderByDate == 1)
        //    {
        //        sqlQuery = sqlQuery.OrderBy(x => x.Slot.StartAt);
        //    }
        //    if (query.OrderByDate is not null && query.OrderByDate == -1)
        //    {
        //        sqlQuery = sqlQuery.OrderBy(x => x.Slot.EndAt);
        //    }
        //    if (query.OrderByName is not null && query.OrderByName == 1)
        //    {
        //        sqlQuery = sqlQuery.OrderBy(x => x.Booker.LastName).ThenBy(x => x.Booker.FirstName); 
        //    }
        //    if (query.OrderByName is not null && query.OrderByName == -1)
        //    {
        //        sqlQuery = sqlQuery.OrderByDescending(x => x.Booker.LastName).ThenByDescending(x => x.Booker.FirstName);
        //    }

        //    var count = await sqlQuery.CountAsync();

        //    var toto = sqlQuery
        //        .AsSplitQuery()
        //        .Skip(query.Start)
        //        .Take(query.PerPage)
        //        .Select(re => re.ToBookingResponseDTO()).ToQueryString();

        //    List<BookingResponseDTO>? result = await sqlQuery
        //        .AsSplitQuery()
        //        .AsNoTracking()
        //        .Skip(query.Start)
        //        .Take(query.PerPage)
        //        .Select(re => re.ToBookingResponseDTO(student))
        //        .ToListAsync();

        //    return (count, result);
        //}
    }
}
