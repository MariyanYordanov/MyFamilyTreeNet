using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MyFamilyTreeNet.Api.Validation
{
    /// <summary>
    /// Validates family name format and prevents inappropriate content
    /// </summary>
    public class ValidFamilyNameAttribute : ValidationAttribute
    {
        private readonly string[] _forbiddenWords = 
        {
            "test", "fake", "dummy", "admin", "root", "system"
        };

        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            var familyName = value.ToString()?.Trim();
            
            if (string.IsNullOrEmpty(familyName))
                return true; // Let [Required] handle empty values

            // Check minimum length
            if (familyName.Length < 2)
            {
                ErrorMessage = "Family name must be at least 2 characters long.";
                return false;
            }

            // Check maximum length
            if (familyName.Length > 50)
            {
                ErrorMessage = "Family name cannot exceed 50 characters.";
                return false;
            }

            // Check for forbidden words
            foreach (var word in _forbiddenWords)
            {
                if (familyName.ToLower().Contains(word))
                {
                    ErrorMessage = $"Family name cannot contain '{word}'.";
                    return false;
                }
            }

            // Check for valid characters (letters including Cyrillic, spaces, hyphens, apostrophes)
            if (!Regex.IsMatch(familyName, @"^[\p{L}\s\-']+$"))
            {
                ErrorMessage = "Името на семейството може да съдържа само букви, интервали, тирета и апострофи.";
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Validates relationship type
    /// </summary>
    public class ValidRelationshipTypeAttribute : ValidationAttribute
    {
        private readonly string[] _validRelationships = 
        {
            "Parent", "Child", "Spouse", "Sibling", "Grandparent", "Grandchild",
            "Uncle", "Aunt", "Nephew", "Niece", "Cousin", "Other"
        };

        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            var relationship = value.ToString();
            
            if (string.IsNullOrEmpty(relationship))
                return true; // Let [Required] handle empty values

            if (!_validRelationships.Contains(relationship))
            {
                ErrorMessage = $"Invalid relationship type. Valid types are: {string.Join(", ", _validRelationships)}.";
                return false;
            }

            return true;
        }
    }
}