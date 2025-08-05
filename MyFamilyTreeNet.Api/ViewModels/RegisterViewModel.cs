using System.ComponentModel.DataAnnotations;
public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email адресът е задължителен")]
        [EmailAddress(ErrorMessage = "Невалиден email адрес")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Паролата е задължителна")]
        [MinLength(8, ErrorMessage = "Паролата трябва да е поне 8 символа")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Паролата трябва да съдържа поне 1 малка буква, 1 главна буква, 1 цифра и 1 специален символ (@$!%*?&)")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Потвърждението на паролата е задължително")]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат")]
        public required string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Името е задължително")]
        [MinLength(2, ErrorMessage = "Името трябва да е поне 2 символа")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Презимето е задължително")]
        [MinLength(2, ErrorMessage = "Презимето трябва да е поне 2 символа")]
        public required string MiddleName { get; set; }

        [Required(ErrorMessage = "Фамилията е задължителна")]
        [MinLength(2, ErrorMessage = "Фамилията трябва да е поне 2 символа")]
        public required string LastName { get; set; }
    }
    