﻿using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Utilities;

namespace TerminalApi.Models
{
    public class OrderResponseForTeacherDTO
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset? PaymentDate { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset? CreatedAt { get; set; }
        public EnumBookingStatus Status { get; set; }
        public string PaymentMethod { get; set; }
        public ICollection<BookingResponseDTO>? Bookings { get; set; }
        public UserResponseDTO? Booker { get; set; }

        [Precision(18, 2)]
        public decimal TotalOriginalPrice { get; set; }

        [Precision(18, 2)]
        public decimal TotalDiscountedPrice { get; set; }

        [Precision(18, 2)]
        public decimal TotalReduction { get; set; }
        public string? PaymentIntent { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }


    }
}
