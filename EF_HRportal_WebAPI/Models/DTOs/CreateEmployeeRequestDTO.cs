using EF_HRportal_WebAPI.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace EF_HRportal_WebAPI.Models.DTOs
{
    public class CreateEmployeeRequestDTO
    {
        [Required(ErrorMessage ="Name field cannot be empty!!")]
        [MinLength(1,ErrorMessage ="Name should contain a minimum of 1 character")]
        [MaxLength(250, ErrorMessage = "Name should contain a maximum of 250 characters")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage ="Email address cannot be empty")]
        [EmailValidation]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage ="Password cannot be empty")]
        [DataType(DataType.Password)]
        [PasswordValidation(ErrorMessage = "Password must meet certain criteria.")]
        public string Password { get; set; } = null!;

        [PhoneNumberValidation]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Salary cannot be empty")]
        [SalaryValidation]
        public int? Salary { get; set; }

        [Required(ErrorMessage = "Department cannot be empty")]
        public string Department { get; set; } = null!;

        [Required(ErrorMessage ="Provide the Date of Joining of the employee!")]
        public DateTime DateOfJoining { get; set; }

        [Required(ErrorMessage ="Provide the Manager ID for the employee!")]
        public int ManagerId { get; set; }
    }
}
