using System.ComponentModel.DataAnnotations;

namespace Tenisu.Application.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class LastArrayAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not List<int> list) return ValidationResult.Success;

        if (list.Count > 5)
            return new ValidationResult("The last array cannot contain more than 5 elements.");

        if (list.Any(x => x != 0 && x != 1))
            return new ValidationResult("Each element in last must be 0 or 1.");

        return ValidationResult.Success;
    }
}
