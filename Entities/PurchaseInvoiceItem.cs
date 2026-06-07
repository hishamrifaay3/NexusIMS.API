namespace NexusIMS.API.Entities
{
    public class PurchaseInvoiceItem
    {
        public int Id { get; set; }

        public int PurchaseInvoiceId { get; set; }
        public virtual PurchaseInvoice PurchaseInvoice { get; set; } = default!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = default!;

        public int Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
