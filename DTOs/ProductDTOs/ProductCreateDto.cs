using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.DTOs.ProductDTOs
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "سعر المنتج مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون السعر أكبر من صفر")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "يجب اختيار قسم للمنتج")]
        public int CategoryId { get; set; }
    }
}