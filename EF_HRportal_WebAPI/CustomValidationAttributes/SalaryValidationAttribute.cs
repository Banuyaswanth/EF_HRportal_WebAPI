using System.ComponentModel.DataAnnotations;

namespace EF_HRportal_WebAPI.CustomValidationAttributes
{
    public class SalaryValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
        {
            var Salary = int.Parse(value.ToString());
            if(Salary >= 300000)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("Salary cannot be less than 3LPA..!!");
        }
    }
}
