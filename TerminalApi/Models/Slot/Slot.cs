﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.User;

namespace TerminalApi.Models.Slots
{
    public class Slot
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [ForeignKey(nameof(Creator))]
        public string CreatedById { get; set; }
        public UserApp Creator { get; set; }
        public Booking? Booking { get; set; }
        [Required]
        [Column(TypeName = "decimal(18, 6)")]
        public decimal Price { get; set; }
        public int? Reduction { get; set; }
    }
}
