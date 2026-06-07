namespace NexusIMS.API.DTOs.DashboardDTOs
{
    public class LowStockAlertDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
    }
}
