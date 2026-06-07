namespace NexusIMS.API.DTOs.DashboardDTOs
{
    public class DashboardDto
    {
        public decimal TotalSales { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal TotalReturns { get; set; } // المرتجعات المخصومة
        public decimal TotalNetProfit { get; set; } // (المبيعات - المشتريات - المرتجعات)
        public int InvoicesCountWithinPeriod { get; set; }

        // 2. التحليلات المتقدمة (الأكثر مبيعاً، الموردين، العملاء)
        public List<LowStockAlertDto> LowStockProducts { get; set; } = new();
        public List<TopProductDto> TopSellingProducts { get; set; } = new();
        public List<TopSupplierDto> TopSuppliers { get; set; } = new();
        public List<TopCustomerDto> TopCustomers { get; set; } = new();
    }
}
