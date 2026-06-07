namespace NexusIMS.API.DTOs.DashboardDTOs
{
    public class TopProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenueGenerated { get; set; }
    }
}
