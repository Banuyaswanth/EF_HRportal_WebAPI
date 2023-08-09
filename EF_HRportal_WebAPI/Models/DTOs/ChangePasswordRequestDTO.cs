using EF_HRportal_WebAPI.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace EF_HRportal_WebAPI.Models.DTOs
{
    public class ChangePasswordRequestDTO
    {
        [Required(ErrorMessage = "Email cannot be empty!!")]
        public string Email { get; set; }

        [Required(ErrorMessage ="Old Password cannot be empty!!")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage ="New Password cannot be empty!!")]
        [DataType(DataType.Password)]
        [PasswordValidation]
        public string NewPassword { get; set; }
    }
}
