using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TerminalApi.Models
{
    public class TVARate
    {
        [Key]
        public Guid Id { get; set; }
        public DateTimeOffset StartAt { get; set; }

        [Precision(18, 2)]
        public decimal Rate { get; set; }
    }
}
