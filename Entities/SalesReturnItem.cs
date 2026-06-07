namespace NexusIMS.API.Entities
{
    public class SalesReturnItem
    {
        public int Id { get; set; }

        public int SalesReturnId { get; set; }
        public virtual SalesReturn SalesReturn { get; set; } = default!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = default!;

        public int Quantity { get; set; } 
        public decimal UnitPrice { get; set; } 
        public decimal TotalPrice { get; set; } 
    }
}
