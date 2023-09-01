using EF_HRportal_WebAPI.CustomValidationAttributes;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System.ComponentModel.DataAnnotations;

namespace EF_HRportal_WebAPI.Models.DTOs
{
    public class ChangePasswordRequestDto
    {
        [Required(ErrorMessage = "Email cannot be empty!!")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Old Password cannot be empty!!")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = null!;

        [Required(ErrorMessage = "New Password cannot be empty!!")]
        [DataType(DataType.Password)]
        [PasswordValidation]
        public string NewPassword { get; set; } = null!;
    }
}
