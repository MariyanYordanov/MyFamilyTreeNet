using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MyFamilyTreeNet.Api.Validation
{
    /// <summary>
    /// Validates title format - allows Cyrillic and Latin letters
    /// </summary>
    public class ValidTitleAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            var title = value.ToString()?.Trim();
            
            if (string.IsNullOrEmpty(title))
                return true;

            // Check minimum length
            if (title.Length < 2)
            {
                ErrorMessage = "Заглавието трябва да бъде поне 2 символа.";
                return false;
            }

            // Check maximum length
            if (title.Length > 200)
            {
                ErrorMessage = "Заглавието не може да надвишава 200 символа.";
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Validates description format - allows Cyrillic and Latin letters
    /// </summary>
    public class ValidDescriptionAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            var description = value.ToString()?.Trim();
            
            if (string.IsNullOrEmpty(description))
                return true;

            // Check maximum length
            if (description.Length > 2000)
            {
                ErrorMessage = "Описанието не може да надвишава 2000 символа.";
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Validates username format - allows Cyrillic and Latin letters
    /// </summary>
    public class ValidUsernameAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            var username = value.ToString()?.Trim();
            
            if (string.IsNullOrEmpty(username))
                return true;

            // Check minimum length
            if (username.Length < 3)
            {
                ErrorMessage = "Потребителското име трябва да бъде поне 3 символа.";
                return false;
            }

            // Check maximum length
            if (username.Length > 30)
            {
                ErrorMessage = "Потребителското име не може да надвишава 30 символа.";
                return false;
            }

            // Allow letters (including Cyrillic), numbers, underscore and dash
            if (!Regex.IsMatch(username, @"^[\p{L}0-9_-]+$"))
            {
                ErrorMessage = "Потребителското име може да съдържа само букви, цифри, долна черта и тире.";
                return false;
            }

            return true;
        }
    }
}