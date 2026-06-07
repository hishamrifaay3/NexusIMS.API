namespace NexusIMS.API.Entities
{
    public class PurchaseInvoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;

        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; } = default!;

        public int WarehouseId { get; set; } // المخزن اللي البضاعة هتدخل فيه وتزوده
        public virtual Warehouse Warehouse { get; set; } = default!;

        public decimal TotalAmount { get; set; } // إجمالي الفاتورة
        public string Remarks { get; set; } = string.Empty;

        public string CreatedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser CreatedByUser { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
    }
}
