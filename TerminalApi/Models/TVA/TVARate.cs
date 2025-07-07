using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TerminalApi.Models
{
    public class TVARate
    {
        public Guid Id { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public decimal Rate { get; set; }
    }
}
