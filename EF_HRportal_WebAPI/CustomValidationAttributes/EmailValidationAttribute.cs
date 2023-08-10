using System.ComponentModel.DataAnnotations;

namespace EF_HRportal_WebAPI.CustomValidationAttributes
{
    public class EmailValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var email = value.ToString();
            if(value == null)
            {
                return new ValidationResult(ErrorMessage = "Email Cannot be empty!!");
            }
            if(email == string.Empty) 
            {
                return new ValidationResult(ErrorMessage = "Email Cannot be empty!!");
            }
            if(email.Any(char.IsWhiteSpace))
            {
                return new ValidationResult(ErrorMessage = "Email cannot contain a whitespace");
            }
            if(!email.ToLower().EndsWith("@gmail.com"))
            {
                return new ValidationResult(ErrorMessage = "Email should end with the @gmail.com domain name");
            }
            return ValidationResult.Success;
        }
    }
}
