﻿using TerminalApi.Models.Slots;

namespace TerminalApi.Models.Slots
{
    public static class SlotExtension
    {
        public static Slot ToSlot(this SlotCreateDTO slotCreateDTO, string userId)
        {
            return new Slot
            {
                StartAt = slotCreateDTO.StartAt,
                EndAt = slotCreateDTO.EndAt,
                CreatedAt = DateTime.Now,
                CreatedById = userId,
                Price = slotCreateDTO.Price,
                Reduction = slotCreateDTO.Reduction,
                Type = slotCreateDTO.Type,
            };
        }

        public static SlotResponseDTO ToResponseDTO(this Slot slot)
        {
            return new SlotResponseDTO
            {
                Id = slot.Id,
                StartAt = slot.StartAt,
                EndAt = slot.EndAt,
                CreatedAt = slot.CreatedAt,
                CreatedById = slot.CreatedById,
                Price = slot.Price,
                Reduction = slot.Reduction,
                Type = slot.Type,
                StudentFirstName = slot.Booking?.Booker.FirstName ?? "",
                StudentLastName = slot.Booking?.Booker.LastName ?? "",
                StudentImgUrl = slot.Booking?.Booker.ImgUrl,
            };
        }
    }
}
