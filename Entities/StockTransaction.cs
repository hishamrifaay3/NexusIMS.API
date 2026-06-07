namespace NexusIMS.API.Entities
{
    public class StockTransaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = default!;

        public int WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set; } = default!;
        public int Quantity { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public TransactionType TransactionType { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser CreatedByUser { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
