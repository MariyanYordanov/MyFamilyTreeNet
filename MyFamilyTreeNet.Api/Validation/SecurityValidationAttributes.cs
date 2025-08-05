using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MyFamilyTreeNet.Api.Validation
{
    /// <summary>
    /// Validates that a string doesn't contain HTML or script tags for XSS prevention
    /// </summary>
    public class NoHtmlAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            var input = value.ToString();
            
            if (string.IsNullOrEmpty(input))
                return true;

            // Check for HTML tags
            if (Regex.IsMatch(input, @"<[^>]*>"))
            {
                ErrorMessage = "HTML tags are not allowed.";
                return false;
            }

            // Check for script content
            if (Regex.IsMatch(input, @"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                ErrorMessage = "Script content is not allowed.";
                return false;
            }

            // Check for javascript: URLs
            if (Regex.IsMatch(input, @"javascript:", RegexOptions.IgnoreCase))
            {
                ErrorMessage = "JavaScript URLs are not allowed.";
                return false;
            }

            // Check for event handlers
            if (Regex.IsMatch(input, @"on\w+\s*=", RegexOptions.IgnoreCase))
            {
                ErrorMessage = "Event handlers are not allowed.";
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Validates strong password requirements
    /// </summary>
    public class StrongPasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            var password = value.ToString();
            
            if (string.IsNullOrEmpty(password))
                return true; // Let [Required] handle empty values

            var errors = new List<string>();

            // Minimum length
            if (password.Length < 8)
                errors.Add("at least 8 characters");

            // Must contain uppercase
            if (!password.Any(char.IsUpper))
                errors.Add("at least one uppercase letter");

            // Must contain lowercase
            if (!password.Any(char.IsLower))
                errors.Add("at least one lowercase letter");

            // Must contain digit
            if (!password.Any(char.IsDigit))
                errors.Add("at least one number");

            // Must contain special character
            if (!password.Any(ch => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(ch)))
                errors.Add("at least one special character");

            // Check for common weak passwords
            var commonPasswords = new[] { "password", "123456", "qwerty", "admin", "letmein" };
            if (commonPasswords.Any(common => password.ToLower().Contains(common)))
                errors.Add("no common weak patterns");

            if (errors.Any())
            {
                ErrorMessage = $"Password must contain {string.Join(", ", errors)}.";
                return false;
            }

            return true;
        }
    }
}