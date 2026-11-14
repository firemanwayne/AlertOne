using System.ComponentModel.DataAnnotations;

namespace AlertOne.Core.Attributes;

/// <summary>
/// Provides a validation attribute that ensures a GUID value is not empty when applied to a property or field.
/// </summary>
/// <remarks>Use this attribute to require that a GUID property or field contains a non-empty value (i.e., not
/// equal to Guid.Empty) during validation. This is commonly used in data models to enforce that a valid, assigned GUID
/// is present. The attribute can be used with data annotation validation frameworks such as ASP.NET Core model
/// validation.</remarks>
public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    /// <summary>
    /// Fails validation when the value is <see cref="Guid.Empty"/>.
    /// </summary>
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is Guid g && g == Guid.Empty)
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}
