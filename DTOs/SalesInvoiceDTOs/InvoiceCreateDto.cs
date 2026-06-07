using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.DTOs.SalesInvoiceDTOs
{
    public class InvoiceCreateDto
    {
        [Required(ErrorMessage = "لا يمكن إنشاء فاتورة فارغة بدون أصناف")]
        public List<InvoiceItemDto> Items { get; set; } = new();

        [Required(ErrorMessage = "يجب ادخال اسم العميل")]
        public string CustomerName { get; set; } = string.Empty;

        public string Remarks { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "نسبة الخصم بين 0 و 100%")]
        public decimal DiscountPercentage { get; set; } = 0; 

        [Range(0, 100, ErrorMessage = "نسبة الضريبة بين 0 و 100%")]
        public decimal TaxPercentage { get; set; } = 14; 
    }
}
