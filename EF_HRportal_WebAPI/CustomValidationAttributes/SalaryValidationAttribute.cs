using System.ComponentModel.DataAnnotations;

namespace EF_HRportal_WebAPI.CustomValidationAttributes
{
    public class SalaryValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var isValidValue = int.TryParse((value ?? "").ToString(), out var Salary);
            if(isValidValue)
            {
                if (Salary >= 300000)
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult("Salary cannot be less than 3LPA..!!");
            }

            string errorMessage = ErrorMessage = "Salary must be an integer";
            return new ValidationResult(errorMessage);
            
        }
    }
}
