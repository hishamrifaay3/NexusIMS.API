namespace NexusIMS.API.DTOs.PurchaseInvoiceDTOs
{
    public class PurchaseInvoiceResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PurchaseItemReportDto> InvoiceItems { get; set; } = new();
    }
}
