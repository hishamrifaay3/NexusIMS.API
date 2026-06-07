namespace NexusIMS.API.DTOs.StockTransactionDTOs
{
    public class StockTransactionResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int TransactionQuantity { get; set; }
        public int ProductQuantity { get; set; }

    }
}
