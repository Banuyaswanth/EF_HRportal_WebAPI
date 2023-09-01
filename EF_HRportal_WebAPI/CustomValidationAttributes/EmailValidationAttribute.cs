using System.ComponentModel.DataAnnotations;

namespace EF_HRportal_WebAPI.CustomValidationAttributes
{
    public class EmailValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var email = value == null ? "" : value.ToString();
            if (email != null)
            {
                if (email == string.Empty)
                {
                    string errorMessage = ErrorMessage = "Email Cannot be empty!!";
                    return new ValidationResult(errorMessage);
                }
                if (email.Any(char.IsWhiteSpace))
                {
                    string errorMessage = ErrorMessage = "Email cannot contain a whitespace";
                    return new ValidationResult(errorMessage);
                }
                if (!email.ToLower().EndsWith("@gmail.com"))
                {
                    string errorMessage = ErrorMessage = "Email should end with the @gmail.com domain name";
                    return new ValidationResult(errorMessage);
                }
                return ValidationResult.Success;
            }

            string errorMessage1 = ErrorMessage = "Email Cannot be empty!!";
            return new ValidationResult(errorMessage1);
        }
    }
}
