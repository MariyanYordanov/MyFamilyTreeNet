using System.ComponentModel.DataAnnotations;

namespace MyFamilyTreeNet.Api.Validation
{
    /// <summary>
    /// Validates that birth date is not in the future and person is not older than 150 years
    /// </summary>
    public class ValidBirthDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true; // Allow null values, use [Required] for mandatory fields

            if (value is DateTime birthDate)
            {
                var today = DateTime.Today;
                var maxAge = today.AddYears(-150);

                // Birth date cannot be in the future
                if (birthDate > today)
                {
                    ErrorMessage = "Birth date cannot be in the future.";
                    return false;
                }

                // Person cannot be older than 150 years
                if (birthDate < maxAge)
                {
                    ErrorMessage = "Birth date cannot be more than 150 years ago.";
                    return false;
                }

                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Validates that death date is after birth date and not in the future
    /// </summary>
    public class ValidDeathDateAttribute : ValidationAttribute
    {
        private readonly string _birthDatePropertyName;

        public ValidDeathDateAttribute(string birthDatePropertyName)
        {
            _birthDatePropertyName = birthDatePropertyName;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success; // Allow null values

            if (value is DateTime deathDate)
            {
                var today = DateTime.Today;

                // Death date cannot be in the future
                if (deathDate > today)
                {
                    return new ValidationResult("Death date cannot be in the future.");
                }

                // Get birth date property for comparison
                var birthDateProperty = validationContext.ObjectType.GetProperty(_birthDatePropertyName);
                if (birthDateProperty != null)
                {
                    var birthDateValue = birthDateProperty.GetValue(validationContext.ObjectInstance);
                    if (birthDateValue is DateTime birthDate)
                    {
                        // Death date must be after birth date
                        if (deathDate <= birthDate)
                        {
                            return new ValidationResult("Death date must be after birth date.");
                        }
                    }
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid death date format.");
        }
    }

    /// <summary>
    /// Validates person name format
    /// </summary>
    public class ValidPersonNameAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            var name = value.ToString()?.Trim();
            
            if (string.IsNullOrEmpty(name))
                return true; // Let [Required] handle empty values

            // Check minimum length
            if (name.Length < 1)
            {
                ErrorMessage = "Name must be at least 1 character long.";
                return false;
            }

            // Check maximum length
            if (name.Length > 50)
            {
                ErrorMessage = "Името не може да надвишава 50 символа.";
                return false;
            }

            // Check for valid characters (letters including Cyrillic, spaces, hyphens, apostrophes)
            if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[\p{L}\s\-']+$"))
            {
                ErrorMessage = "Името може да съдържа само букви, интервали, тирета и апострофи.";
                return false;
            }

            // Must start with a letter
            if (!char.IsLetter(name[0]))
            {
                ErrorMessage = "Името трябва да започва с буква.";
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Validates gender values
    /// </summary>
    public class ValidGenderAttribute : ValidationAttribute
    {
        private readonly string[] _validGenders = { "Male", "Female", "Other", "Prefer not to say" };

        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            var gender = value.ToString();
            
            if (string.IsNullOrEmpty(gender))
                return true; // Let [Required] handle empty values

            if (!_validGenders.Contains(gender))
            {
                ErrorMessage = $"Invalid gender. Valid options are: {string.Join(", ", _validGenders)}.";
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Validates that user is old enough (GDPR compliance)
    /// </summary>
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            if (value is DateTime birthDate)
            {
                var age = DateTime.Today.Year - birthDate.Year;
                if (birthDate.Date > DateTime.Today.AddYears(-age))
                    age--;

                if (age < _minimumAge)
                {
                    ErrorMessage = $"You must be at least {_minimumAge} years old.";
                    return false;
                }
            }

            return true;
        }
    }
}