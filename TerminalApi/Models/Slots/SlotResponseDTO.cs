using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Utilities;

namespace TerminalApi.Models.Slots
{
    public class SlotResponseDTO
    {
        public Guid Id { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedById { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }

        [Precision(18, 2)]
        public decimal DiscountedPrice { get; set; }
        public int? Reduction { get; set; }
        public EnumSlotType Type { get; set; }
        public string? StudentId { get; set; }
        public string? StudentFirstName { get; set; }
        public string? StudentLastName { get; set; }
        public string? StudentImgUrl { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public EnumTypeHelp? TypeHelp { get; set; }

        //public SlotResponseDTO(Slot ad)
        //{
        //    Id = ad.Id;
        //    StartAt = ad.StartAt;
        //    EndAt = ad.EndAt;
        //    Price = ad.Price;
        //    DiscountedPrice = ad.DiscountedPrice;
        //    Reduction = ad.Reduction;
        //    StudentFirstName = ad.Booking?.Booker.FirstName ?? null;
        //    StudentLastName = ad.Booking?.Booker.LastName ?? null;
        //    StudentImgUrl = ad.Booking?.Booker.ImgUrl ?? null;
        //    StudentId = ad.Booking?.BookedById ?? null;
        //    Subject = ad.Booking?.Subject ?? null;
        //    Description = ad.Booking?.Description ?? null;
        //    TypeHelp = ad.Booking?.TypeHelp ?? null;
        //}
    }
}
