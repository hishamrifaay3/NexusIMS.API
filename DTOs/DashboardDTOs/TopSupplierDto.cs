namespace NexusIMS.API.DTOs.DashboardDTOs
{
    public class TopSupplierDto
    {
        public string SupplierName { get; set; } = string.Empty;
        public int TotalPurchaseOrders { get; set; }
        public decimal TotalAmountPaid { get; set; }
    }
}
