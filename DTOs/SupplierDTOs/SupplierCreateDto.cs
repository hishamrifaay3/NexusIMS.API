using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.DTOs.SupplierDTOs
{
    public class SupplierCreateDto
    {
        [Required(ErrorMessage = "إسم المورد حقل إلزامي.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "إسم المورد يجب أن يكون بين 3 إلى 100 حرف.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف حقل إلزامي.")]
        [Phone(ErrorMessage = "تنسيق رقم الهاتف غير صحيح.")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "رقم الهاتف يجب أن يكون رقم مصري صحيح مكون من 11 رقم.")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "تنسيق البريد الإلكتروني غير صحيح.")]
        [StringLength(150, ErrorMessage = "البريد الإلكتروني لا يمكن أن يتخطى 150 حرف.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "العنوان لا يمكن أن يتخطى 250 حرف.")]
        public string Address { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "الرقم الضريبي لا يمكن أن يتخطى 50 حرف.")]
        public string TaxNumber { get; set; } = string.Empty; // الرقم الضريبي للمورد (اختياري لكن لو اتبعت يتأكد من طوله)
    }
}