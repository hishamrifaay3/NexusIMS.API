namespace NexusIMS.API.DTOs.StockTransactionDTOs
{
    public class StockTransactionListDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
