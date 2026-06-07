using NexusIMS.API.Entities;
using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.DTOs.SupplierDTOs
{
    public class SupplierUpdateDto
    {
        // ❌ شيلنا الـ Id تماماً لأنك هتستقبله كـ Parameter في الـ Controller
        // ❌ شيلنا [Required] من كل الحقول عشان يقدر يعدل حقل واحد بس براحته

        [StringLength(100, MinimumLength = 3, ErrorMessage = "إسم المورد يجب أن يكون بين 3 إلى 100 حرف.")]
        public string? Name { get; set; } 

        [Phone(ErrorMessage = "تنسيق رقم الهاتف غير صحيح.")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "رقم الهاتف يجب أن يكون رقم مصري صحيح مكون من 11 رقم.")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "تنسيق البريد الإلكتروني غير صحيح.")]
        [StringLength(150, ErrorMessage = "البريد الإلكتروني لا يمكن أن يتخطى 150 حرف.")]
        public string? Email { get; set; }

        [StringLength(250, ErrorMessage = "العنوان لا يمكن أن يتخطى 250 حرف.")]
        public string? Address { get; set; }

        [StringLength(50, ErrorMessage = "الرقم الضريبي لا يمكن أن يتخطى 50 حرف.")]
        public string? TaxNumber { get; set; }

        public bool? IsActive { get; set; }

        public void UpdateEntity(Supplier supplier)
        {
            supplier.Name = !string.IsNullOrWhiteSpace(Name) ? Name : supplier.Name;
            supplier.Phone = !string.IsNullOrWhiteSpace(Phone) ? Phone : supplier.Phone;

            if (Email != null) supplier.Email = Email;
            if (Address != null) supplier.Address = Address;
            if (TaxNumber != null) supplier.TaxNumber = TaxNumber;
            if (IsActive.HasValue) supplier.IsActive = IsActive.Value;
        }
    }
}