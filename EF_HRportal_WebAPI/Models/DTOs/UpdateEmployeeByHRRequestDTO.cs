using EF_HRportal_WebAPI.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace EF_HRportal_WebAPI.Models.DTOs
{
    public class UpdateEmployeeByHRRequestDTO
    {
        [Required(ErrorMessage = "Name field cannot be empty!!")]
        [MinLength(1, ErrorMessage = "Name should contain a minimum of 1 character")]
        [MaxLength(250, ErrorMessage = "Name should contain a maximum of 250 characters")]
        public string Name { get; set; }

        [PhoneNumberValidation]
        public string Phone { get; set; }

        [Required]
        [SalaryValidation]
        public int Salary { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        public int ManagerId { get; set; }
    }
}
