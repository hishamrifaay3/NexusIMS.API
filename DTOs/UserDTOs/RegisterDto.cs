using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.DTOs.UserDTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage ="رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        public string Phone { get; set; } = string.Empty;
        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "كلمة المرور لا تقل عن 8 أحرف")]
        public string Password { get; set; } = string.Empty;
        public int? WarehouseId { get; set; }

        [Required(ErrorMessage = "يجب تحديد صلاحية المستخدم")]
        public string Role { get; set; } = string.Empty;
    }
}
