using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EF_HRportal_WebAPI.CustomValidationAttributes
{
    public class PasswordValidationAttribute : ValidationAttribute
    {
        public string Pattern  => @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@!#\$%\^&\*])[A-Za-z\d@!#\$%\^&\*]{8,}$";
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var inputValue = value.ToString() ?? "";
                if (!inputValue.Any(Char.IsWhiteSpace) && Regex.IsMatch(inputValue, Pattern))
                {
                    return ValidationResult.Success;
                }
                string errorMessage1 = ErrorMessage = "Password should contain at least 8 Characters.Password should contain at least 1 Uppercase letter.Password should contain at least 1 Lowercase Letter.Password should contain at least 1 Special Character.Password cannot contain a whitespace";
                return new ValidationResult(errorMessage1);
            }
            string errorMessage = ErrorMessage = "Password cannot be empty!!";
            return new ValidationResult(errorMessage);
        }
    }
}
