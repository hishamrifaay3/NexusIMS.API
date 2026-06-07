using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.DTOs.PurchaseInvoiceDTOs
{
    public class PurchaseInvoiceCreateDto
    {
        [Required(ErrorMessage = "يجب تحديد المورد")]
        public int SupplierId { get; set; }
        public int? WarehouseId { get; set; }

        [Required(ErrorMessage = "لا يمكن عمل فاتورة مشتريات بدون أصناف")]
        public List<PurchaseItemDto> Items { get; set; } = new();

        public string Remarks { get; set; } = string.Empty;
    }
}
