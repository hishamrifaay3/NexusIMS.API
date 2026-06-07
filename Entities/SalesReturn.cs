namespace NexusIMS.API.Entities
{
    public class SalesReturn
    {
        public int Id { get; set; }
        public string ReturnNumber { get; set; } = string.Empty; // رقم مستند المرتجع تلقائي RET-XXXX

        public int SalesInvoiceId { get; set; } // ربط بالفاتورة الأصلية
        public virtual SalesInvoice SalesInvoice { get; set; } = default!;

        public int WarehouseId { get; set; } // المخزن الذي دخلت إليه البضاعة المرتجعة
        public virtual Warehouse Warehouse { get; set; } = default!;

        public string CreatedByUserId { get; set; } = string.Empty; // مين اللي عمل المرتجع (الـ Audit الحقيقي)
        public virtual ApplicationUser CreatedByUser { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public decimal TotalReturnAmount { get; set; } // إجمالي المبلغ المسترد للعميل
        public string Remarks { get; set; } = string.Empty;

        // تفاصيل الأصناف المرتجعة داخل هذا المستند
        public virtual ICollection<SalesReturnItem> Items { get; set; } = new List<SalesReturnItem>();
    }
}
