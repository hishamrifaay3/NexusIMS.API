namespace NexusIMS.API.DTOs.DashboardDTOs
{
    public class TopCustomerDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty; // المخزن اللي متعود يشتري منه
        public int TotalVisits { get; set; } // عمل كام فاتورة
        public decimal TotalSpent { get; set; } // دفع كام اجمالي
    }
}
