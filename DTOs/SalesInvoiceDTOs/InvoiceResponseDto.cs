namespace NexusIMS.API.DTOs.SalesInvoiceDTOs
{
    public class InvoiceResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }  
        public decimal Tax { get; set; }   
        public decimal Discount { get; set; }  
        public decimal FinalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<InvoiceItemReportDto> InvoiceItems { get; set; } = new();
    }
}
