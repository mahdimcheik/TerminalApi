using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models.Adresse;
using TerminalApi.Models.Slots;

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
                    x.Id == Guid.Parse(slotUpdateDTO.Id)
                    && x.CreatedById == userId
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

        public async Task<List<SlotResponseDTO>?> GetSlotsByCreator(string teacherId)
        {
            return await context
                .Slots.Where(ad => ad.CreatedById == teacherId)
                .Select(ad => ad.ToResponseDTO())
                .ToListAsync();
        }
    }
}
