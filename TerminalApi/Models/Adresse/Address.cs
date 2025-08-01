﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TerminalApi.Models
{
    public class Address
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public int StreetNumber { get; set; }
        [Required]
        [StringLength(100)]
        public string Street { get; set; }

        [StringLength(150)]
        public string StreetLine2 { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [StringLength(150)]
        public string State { get; set; }

        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(50)]
        public string? Country { get; set; } = "France";

        [ForeignKey(nameof(user))]
        public string UserId { get; set; }
        public UserApp user { get; set; }
        public AddressTypeEnum AddressType { get; set; }
    }

    public enum AddressTypeEnum
    {
        Home = 1,
        Work = 2,
        Billing = 3,
        Shipping = 4
    }
}