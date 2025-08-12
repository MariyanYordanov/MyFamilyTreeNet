using System.ComponentModel.DataAnnotations;

namespace MyFamilyTreeNet.Api.Validation
{
    /// <summary>
    /// Validates file size for uploads
    /// </summary>
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;

        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    var maxSizeMB = _maxFileSize / 1048576;
                    ErrorMessage = $"File size cannot exceed {maxSizeMB} MB.";
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Validates allowed file extensions
    /// </summary>
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(params string[] extensions)
        {
            _extensions = extensions;
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!_extensions.Contains(extension))
                {
                    ErrorMessage = $"Only {string.Join(", ", _extensions)} files are allowed.";
                    return false;
                }
            }

            return true;
        }
    }
}