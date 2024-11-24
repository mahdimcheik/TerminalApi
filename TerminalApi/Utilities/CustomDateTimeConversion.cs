using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TerminalApi.Utilities
{
    public class CustomDateTimeConversion : ValueConverter<DateTimeOffset, DateTime>
    {
        public CustomDateTimeConversion()
            : base(
            // Convert DateTimeOffset to DateTime (UTC) for storage
            dto => dto.UtcDateTime,
            // Convert DateTime (from DB) back to DateTimeOffset (assume it's UTC)
            dt => new DateTimeOffset(dt, TimeSpan.Zero))
            // dt => dt.ToLocalTime())
        {
        }
    }
}
