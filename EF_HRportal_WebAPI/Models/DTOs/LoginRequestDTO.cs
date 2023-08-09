using System.ComponentModel.DataAnnotations;

namespace EF_HRportal_WebAPI.Models.DTOs
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage ="Email cannot be empty..!!")]
        public string Email { get; set; }

        [Required(ErrorMessage ="Password cannot be empty..!!")]
        public string Password { get; set; }
    }
}
