using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.DTOs.PurchaseInvoiceDTOs
{
    public class PurchaseItemDto
    {
        [Required(ErrorMessage = "يجب تحديد المنتج")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "يجب إدخال الكمية المشتراة")]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون 1 أو أكثر")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "يجب إدخال سعر التكلفة")]
        [Range(0.01, double.MaxValue, ErrorMessage = "سعر التكلفة يجب أن يكون أكبر من صفر")]
        public decimal CostPrice { get; set; } // سعر الشراء من المورد
    }
}
