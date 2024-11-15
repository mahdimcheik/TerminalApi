using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TerminalApi.Models.Formations
{
    public class FormationCreateDTO
    {
        [Required]
        public string Company { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "nvarchar(125)")]
        public string Title { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Country { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartAt { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndAt { get; set; }
    }
}
