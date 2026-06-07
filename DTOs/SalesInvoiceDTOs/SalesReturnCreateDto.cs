using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.DTOs.SalesInvoiceDTOs
{
    public class SalesReturnCreateDto
    {
        [Required(ErrorMessage ="يجب ادخال رقم الفاتوره")]
        public int SalesInvoiceId { get; set; }

        [Required(ErrorMessage ="يجب ادخال الاصناف المراد راجاعها")]
        public List<InvoiceItemDto> ItemsToReturn { get; set; } = new();

        public string Remarks { get; set; } = string.Empty;
    }
}
