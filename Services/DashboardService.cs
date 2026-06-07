using Microsoft.EntityFrameworkCore;
using NexusIMS.API.Data;
using NexusIMS.API.DTOs.DashboardDTOs;

namespace NexusIMS.API.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardData(
            DateTime? startDate, DateTime? endDate, int? userWarehouseId, string userRole)
        {
            var isClientLimited = userRole != "Admin" && userRole != "Manager" 
                && userRole != "GeneralAccountant";

            // ضبط تواريخ افتراضية لو الـ Frontend مبعتش (مثلا آخر 30 يوم)
            var actualStartDate = startDate ?? DateTime.UtcNow.AddDays(-30);
            var actualEndDate = endDate ?? DateTime.UtcNow;

            // 1. بناء الكويريهات الأساسية (بدون Execute فوراً لضمان سرعة الـ Query Pipelines)
            var salesQuery = _context.SalesInvoices.AsNoTracking()
                .Where(i => i.CreatedAt >= actualStartDate && i.CreatedAt <= actualEndDate);

            var purchaseQuery = _context.PurchaseInvoices.AsNoTracking()
                .Where(p => p.CreatedAt >= actualStartDate && p.CreatedAt <= actualEndDate);

            var returnsQuery = _context.SalesReturns.AsNoTracking()
                .Where(r => r.CreatedAt >= actualStartDate && r.CreatedAt <= actualEndDate);

            var stockQuery = _context.Stocks
                .AsNoTracking()
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .AsQueryable();
            var invoiceItemsQuery = _context.InvoiceItems
                .AsNoTracking()
                .Where(item => item.SalesInvoice.CreatedAt >= actualStartDate 
                && item.SalesInvoice.CreatedAt <= actualEndDate);

            // تطبيق حارس الفصل التام للمخازن
            if (isClientLimited)
            {
                salesQuery = salesQuery.Where(i => i.WarehouseId == userWarehouseId);
                purchaseQuery = purchaseQuery.Where(p => p.WarehouseId == userWarehouseId);
                returnsQuery = returnsQuery.Where(r => r.WarehouseId == userWarehouseId);
                stockQuery = stockQuery.Where(s => s.WarehouseId == userWarehouseId);
                invoiceItemsQuery = invoiceItemsQuery.Where(item => item.SalesInvoice.WarehouseId == userWarehouseId);
            }

            // 2. الحسابات المالية الدقيقة للفترة المحددة
            var totalSales = await salesQuery.SumAsync(i => i.FinalAmount);
            var totalPurchases = await purchaseQuery.SumAsync(p => p.TotalAmount);
            var totalReturns = await returnsQuery.SumAsync(r => r.TotalReturnAmount);
            var invoicesCount = await salesQuery.CountAsync();

            // 3. المنتجات القريبة من النفاد (تنبيهات حية - لا تـتأثر بفترة التاريخ لأنها جرد حالي لفرعك)
            var lowStockAlerts = await stockQuery
                .Where(s => s.Quantity <= 10)
                .OrderBy(s => s.Quantity)
                .Select(s => new LowStockAlertDto
                {
                    ProductName = s.Product.Name,
                    WarehouseName = s.Warehouse.Name,
                    CurrentQuantity = s.Quantity
                })
                .Take(5)
                .ToListAsync();

            // 4. المنتجات الأكثر مبيعاً خلال الفترة المحددة
            var topSelling = await invoiceItemsQuery
                .GroupBy(item => item.Product.Name)
                .Select(g => new TopProductDto
                {
                    ProductName = g.Key,
                    TotalQuantitySold = g.Sum(x => x.Quantity),
                    TotalRevenueGenerated = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(5)
                .ToListAsync();

            // 5. الموردين الأكثر تعاملاً (Top Suppliers) خلال الفترة المحددة
            var topSuppliers = await purchaseQuery
                .GroupBy(p => p.Supplier.Name)
                .Select(g => new TopSupplierDto
                {
                    SupplierName = g.Key,
                    TotalPurchaseOrders = g.Count(),
                    TotalAmountPaid = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.TotalAmountPaid)
                .Take(5)
                .ToListAsync();

            // 6. ذكاء البيانات: تحليل العملاء الأكثر شراءً ومن أي مخزن خلال الفترة
            var topCustomers = await salesQuery
                .Where(i => !string.IsNullOrEmpty(i.CustomerName)) // تجاهل الفواتير بدون اسم عميل
                .GroupBy(i => new { i.CustomerName, WarehouseName = i.Warehouse.Name })
                .Select(g => new TopCustomerDto
                {
                    CustomerName = g.Key.CustomerName,
                    WarehouseName = g.Key.WarehouseName,
                    TotalVisits = g.Count(),
                    TotalSpent = g.Sum(x => x.FinalAmount)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(5)
                .ToListAsync();

            // 7. صب البيانات في الـ DTO
            return new DashboardDto
            {
                TotalSales = totalSales,
                TotalPurchases = totalPurchases,
                TotalReturns = totalReturns,
                TotalNetProfit = totalSales - totalPurchases - totalReturns, // الخصم الصافي للمرتجعات
                InvoicesCountWithinPeriod = invoicesCount,
                LowStockProducts = lowStockAlerts,
                TopSellingProducts = topSelling,
                TopSuppliers = topSuppliers,
                TopCustomers = topCustomers
            };
        }
    }
}