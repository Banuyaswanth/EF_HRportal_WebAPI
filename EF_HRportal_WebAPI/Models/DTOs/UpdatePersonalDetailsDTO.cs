using EF_HRportal_WebAPI.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace EF_HRportal_WebAPI.Models.DTOs
{
    public class UpdatePersonalDetailsDto
    {
        [Required]
        public string Name { get; set; } = null!;

        [PhoneNumberValidation]
        public string Phone { get;set; } = null!;
    }
}
