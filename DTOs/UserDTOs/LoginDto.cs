using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.DTOs.UserDTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; } = string.Empty;
    }
}
