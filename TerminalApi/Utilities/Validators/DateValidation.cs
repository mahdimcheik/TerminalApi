using System.ComponentModel.DataAnnotations;

namespace TerminalApi.Utilities.Validators
{
    public class DateRangeAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;

        public DateRangeAttribute(string otherProperty)
        {
            _otherProperty = otherProperty;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var endDate = value as DateTimeOffset?;
            var startProp = validationContext.ObjectType.GetProperty(_otherProperty);

            if (startProp == null)
                return new ValidationResult($"Propriété inconnue: {_otherProperty}");

            var startDate = startProp.GetValue(validationContext.ObjectInstance) as DateTimeOffset?;

            if (startDate != null && endDate != null && startDate >= endDate)
            {
                return new ValidationResult("La date de début doit être antérieure à la date de fin.");
            }

            return ValidationResult.Success!;
        }
    }
}
