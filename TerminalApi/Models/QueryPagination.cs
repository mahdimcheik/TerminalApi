﻿using System.ComponentModel.DataAnnotations.Schema;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

namespace TerminalApi.Models
{
    public class QueryPagination
    {
        public int Start { get; set; }
        public int PerPage { get; set; }
        public string? StudentId { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public short? OrderByName { get; set; } = 0;
        public short? OrderByDate { get; set; } = 0;
    }

    public class OrderPagination
    {
        public int Start { get; set; }
        public int PerPage { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public EnumBookingStatus Status { get; set; }
        public string? BookerId { get; set; }
        public short? OrderByDate { get; set; } = 0;
        public string? SearchField { get; set; }
    }
}
