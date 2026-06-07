namespace NexusIMS.API.Entities
{
    public class SalesInvoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty; 
        public string Remarks { get; set; } = string.Empty; 


        public decimal TotalAmount { get; set; } 
        public decimal Tax { get; set; }      
        public decimal Discount { get; set; }   
        public decimal FinalAmount { get; set; } 

        public string CreatedByUserId { get; set; } = string.Empty; 
        public virtual ApplicationUser User { get; set; } = default!;

        public int WarehouseId { get; set; } 
        public virtual Warehouse Warehouse { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // حالة الفاتورة: 1 = Active, 2 = PartiallyReturned, 3 = FullyReturned
        public int Status { get; set; } = 1;

        public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }
}
