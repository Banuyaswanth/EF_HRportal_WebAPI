using System.ComponentModel.DataAnnotations;

namespace EF_HRportal_WebAPI.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email cannot be empty..!!")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage ="Password cannot be empty..!!")]
        public string Password { get; set; }= null!;
    }
}
