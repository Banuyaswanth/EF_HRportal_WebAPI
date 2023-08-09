using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EF_HRportal_WebAPI.CustomValidationAttributes
{
    public class PasswordValidationAttribute : ValidationAttribute
    {
        public string Pattern  => @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@!#\$%\^&\*])[A-Za-z\d@!#\$%\^&\*]{8,}$";
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if(value == null)
            {
                return new ValidationResult(ErrorMessage = "Password cannot be empty!!");
            }
            var inputValue = value.ToString();
            if(Regex.IsMatch(inputValue,Pattern))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage = "Password should contain at least 8 Characters.Password should contain at least 1 Uppercase letter.Password should contain at least 1 Lowercase Letter.Password should contain at least 1 Special Character");
        }
    }
}
