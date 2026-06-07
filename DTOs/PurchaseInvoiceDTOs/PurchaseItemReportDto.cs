namespace NexusIMS.API.DTOs.PurchaseInvoiceDTOs
{
    public class PurchaseItemReportDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
