using System.ComponentModel.DataAnnotations.Schema;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

namespace TerminalApi.Models
{
    public class QueryPagination
    {
        public int Start { get; set; } = 0;
        public int PerPage { get; set; } = 10;
        public string? StudentId { get; set; }
        public string? SearchWord { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public short? OrderByName { get; set; } = 0;
        public short? OrderByDate { get; set; } = 0;
    }

    public class OrderPagination
    {
        public int Start { get; set; } = 0;
        public int PerPage { get; set; } = 10;
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public EnumBookingStatus? Status { get; set; }
        public string? BookerId { get; set; }
        public short? OrderByDate { get; set; } = 0;
        public string? SearchField { get; set; }
    }
}
