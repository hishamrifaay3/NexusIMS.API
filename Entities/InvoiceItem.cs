namespace NexusIMS.API.Entities
{
    public class InvoiceItem
    {
        public int Id { get; set; }
        public int SalesInvoiceId { get; set; }
        public virtual SalesInvoice SalesInvoice { get; set; } = default!;
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ItemDiscount { get; set; } // قيمة خصم هذا الصنف
        public decimal ItemTax { get; set; }      // قيمة ضريبة هذا الصنف
        public decimal NetPrice { get; set; }     // الصافي الفعلي للسطر (السعر النهائي الموزع)
    }
}
