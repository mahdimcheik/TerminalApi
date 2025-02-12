using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models.Adresse;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.Slots;
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

        public async Task<List<SlotResponseDTO>?> GetSlotsByCreator(
            string teacherId,
            DateTimeOffset fromDate,
            DateTimeOffset toDate
        )
        {
            return await context
                .Slots.Where(ad =>
                    ad.CreatedById == teacherId && ad.StartAt >= fromDate && ad.EndAt <= toDate
                )
                .Select(ad => new SlotResponseDTO
                {
                    Id = ad.Id,
                    StartAt = ad.StartAt,
                    EndAt = ad.EndAt,
                    Price = ad.Price,
                    Reduction = ad.Reduction,
                    StudentFirstName = ad.Booking.Booker.FirstName ,
                    StudentLastName = ad.Booking.Booker.LastName,
                    StudentImgUrl = ad.Booking.Booker.ImgUrl,
                    StudentId = ad.Booking.BookedById
                })
                .ToListAsync();
        }

        public async Task<List<SlotResponseDTO>?> GetSlotsByStudent(
            string userId,
           DateTimeOffset fromDate,
           DateTimeOffset toDate
       )
        {
            return await context
                .Slots.AsSplitQuery().Where(ad =>
                    (ad.CreatedById == EnvironmentVariables.TEACHER_ID && ad.StartAt >= fromDate && ad.EndAt <= toDate && ad.StartAt >=DateTimeOffset.UtcNow && ad.Booking  ==  null)  
                    || (ad.CreatedById ==  EnvironmentVariables.TEACHER_ID && ad.StartAt >= fromDate && ad.EndAt <= toDate && ad.Booking != null && ad.Booking.BookedById == userId)
                )
                .Select(ad => new SlotResponseDTO
                {
                    Id = ad.Id,
                    StartAt = ad.StartAt,
                    EndAt = ad.EndAt,
                    Price = ad.Price,
                    DiscountedPrice = ad.DiscountedPrice,
                    Reduction = ad.Reduction,
                    StudentFirstName = ad.Booking.Booker.FirstName,
                    StudentLastName = ad.Booking.Booker.LastName,
                    StudentImgUrl = ad.Booking.Booker.ImgUrl,
                    StudentId = ad.Booking.BookedById
                })
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
                var slot = await context.Slots.FirstOrDefaultAsync(x =>
                    x.Id == Guid.Parse(slotId) && x.CreatedById == userId
                );
                if (slot is null)
                    throw new Exception("Le créneau n'existe pas");
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

        public async Task<bool> BookSlot(string slotId, string bookerId)
        {
            var slot = await context
                .Slots.Where(x => x.Id == Guid.Parse(slotId) && x.StartAt > DateTimeOffset.UtcNow)
                .Include(x => x.Booking)
                .FirstOrDefaultAsync();
            if (slot is null || slot.Booking is not null)
            {
                return false;
            }

            Booking newBooking = new Booking()
            {
                BookedById = bookerId,
                SlotId = Guid.Parse(slotId),
                CreatedAt = DateTimeOffset.UtcNow
            };
            try
            {
                var res = await context.Bookings.AddAsync(newBooking);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Résérvation non créée");
            }
        }

        public async Task<bool> RemoveReservationByTeacher(string slotId)
        {
            var slot = await context
                .Slots
                .AsSplitQuery()
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
                .Slots
                .AsSplitQuery()
                .Include(x => x.Booking)
                .Where(x => x.Id == Guid.Parse(slotId) && x.Booking.BookedById == studentId)
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
    }
}
