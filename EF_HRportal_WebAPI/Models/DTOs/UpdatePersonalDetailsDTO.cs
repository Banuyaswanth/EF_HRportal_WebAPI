using EF_HRportal_WebAPI.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace EF_HRportal_WebAPI.Models.DTOs
{
    public class UpdatePersonalDetailsDTO
    {
        [Required]
        public string Name { get; set; }

        [PhoneNumberValidation]
        public string Phone { get;set; }
    }
}
