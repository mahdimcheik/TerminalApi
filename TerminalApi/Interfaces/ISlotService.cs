using TerminalApi.Models;
using TerminalApi.Models.Slots;

namespace TerminalApi.Interfaces
{
    public interface ISlotService
    {
        Task<SlotResponseDTO> AddSlot(SlotCreateDTO slotCreateDTO, string userId);
        Task<SlotResponseDTO> UpdateSlot(SlotUpdateDTO slotUpdateDTO, string userId);
        Task<SlotResponseDTO?> GetSlotsById(string slotId);
        Task<List<SlotResponseDTO>?> GetSlotsByCreator(string teacherId, DateTimeOffset fromDate, DateTimeOffset toDate);
        Task<List<SlotResponseDTO>?> GetSlotsByStudent(string userId, DateTimeOffset fromDate, DateTimeOffset toDate);
        Task<bool> DeleteSlot(string userId, string slotId);
    }
} 