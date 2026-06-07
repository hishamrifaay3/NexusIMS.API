using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.DTOs.SalesInvoiceDTOs
{
    public class InvoiceItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية المباعة يجب أن تكون 1 أو أكثر")]
        public int Quantity { get; set; }
    }
}
