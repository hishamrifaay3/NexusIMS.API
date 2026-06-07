using NexusIMS.API.Data;
using NexusIMS.API.DTOs.PurchaseInvoiceDTOs;
using Microsoft.EntityFrameworkCore;
using NexusIMS.API.Entities;

namespace NexusIMS.API.Services
{
    public class PurchaseInvoiceService : IPurchaseInvoiceService
    {
        private readonly ApplicationDbContext _context;

        public PurchaseInvoiceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseInvoiceResponseDto> CreatePurchaseInvoice(
            PurchaseInvoiceCreateDto model, string userId, int? userWarehouseId, string userRole)
        {
            int targetWarehouseId = -1;

            // 1. تحديد المخزن بناءً على الصلاحيات
            if (userRole == "Admin" || userRole == "Manager")
            {
                if (model.WarehouseId == null || model.WarehouseId == 0)
                {
                    return new PurchaseInvoiceResponseDto { IsSuccess = false, Message = "يجب تحديد المخزن المستهدف للتوريد." };
                }
                targetWarehouseId = model.WarehouseId.Value;
            }
            else
            {
                if (userWarehouseId == null)
                {
                    return new PurchaseInvoiceResponseDto { IsSuccess = false, Message = "عذراً، حسابك غير مرتبط بمخزن حالياً لتوريد البضاعة إليه." };
                }
                if (model.WarehouseId != null && model.WarehouseId != userWarehouseId)
                {
                    return new PurchaseInvoiceResponseDto { IsSuccess = false, Message = "غير مصرح لك بالتوريد لمخزن آخر غير المرتبط بحسابك." };
                }
                targetWarehouseId = userWarehouseId.Value;
            }

            // 2. الـ Validations الأولية
            var supplier = await _context.Suppliers.FindAsync(model.SupplierId);
            if (supplier == null || !supplier.IsActive)
                return new PurchaseInvoiceResponseDto { IsSuccess = false, Message = "المورد المحدد غير موجود أو غير نشط." };

            if (model.Items == null || !model.Items.Any())
                return new PurchaseInvoiceResponseDto { IsSuccess = false, Message = "لا يمكن حفظ فاتورة مشتريات فارغة." };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalInvoiceAmount = 0;
                var purchaseItemsToSave = new List<PurchaseInvoiceItem>();
                var itemsReportForResponse = new List<PurchaseItemReportDto>();

                // ✅ إصلاح البج القاتل: نعد من جدول المشتريات نفسه
                var currentYear = DateTime.UtcNow.Year.ToString();
                var invoiceCount = await _context.PurchaseInvoices.MaxAsync(i => (int?)i.Id) ?? 0;
                var invoiceNumber = $"PUR-{currentYear}-{(invoiceCount + 1).ToString("D4")}";

                var warehouse = await _context.Warehouses.FindAsync(targetWarehouseId);

                var purchaseInvoice = new PurchaseInvoice
                {
                    InvoiceNumber = invoiceNumber,
                    SupplierId = model.SupplierId,
                    WarehouseId = targetWarehouseId,
                    CreatedByUserId = userId,
                    Remarks = model.Remarks,
                    TotalAmount = 0, // هيتحدث تحت
                    CreatedAt = DateTime.UtcNow
                };

                // شيلنا الـ SaveChanges الأولى تيسيراً للأداء المتميز وعشان الـ Transaction
                await _context.PurchaseInvoices.AddAsync(purchaseInvoice);

                foreach (var item in model.Items)
                {
                    // تأمين إضافي ضد الكميات الصفرية أو السالبة
                    if (item.Quantity <= 0 || item.CostPrice <= 0)
                    {
                        await transaction.RollbackAsync();
                        return new PurchaseInvoiceResponseDto { IsSuccess = false, Message = "يجب أن تكون الكمية وأسعار التكلفة أكبر من صفر." };
                    }

                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null || product.IsDeleted)
                    {
                        await transaction.RollbackAsync();
                        return new PurchaseInvoiceResponseDto { IsSuccess = false, Message = $"المنتج ذو الرقم {item.ProductId} غير موجود في النظام." };
                    }

                    product.CostPrice = item.CostPrice;

                    // زيادة رصيد المخزن
                    var stock = await _context.Stocks
                        .SingleOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == targetWarehouseId);

                    if (stock == null)
                    {
                        stock = new Stock { ProductId = item.ProductId, WarehouseId = targetWarehouseId, Quantity = 0 };
                        await _context.Stocks.AddAsync(stock);
                    }

                    stock.Quantity += item.Quantity;

                    decimal itemTotal = item.CostPrice * item.Quantity;
                    totalInvoiceAmount += itemTotal;

                    purchaseItemsToSave.Add(new PurchaseInvoiceItem
                    {
                        PurchaseInvoice = purchaseInvoice, // ربط مباشر بالكائن لإن الايدي لسه متولّدش
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        CostPrice = item.CostPrice,
                        TotalPrice = itemTotal
                    });

                    itemsReportForResponse.Add(new PurchaseItemReportDto
                    {
                        ProductName = product.Name,
                        Quantity = item.Quantity,
                        CostPrice = item.CostPrice,
                        TotalPrice = itemTotal
                    });

                    // كارت الصنف
                    await _context.StockTransactions.AddAsync(new StockTransaction
                    {
                        ProductId = item.ProductId,
                        WarehouseId = targetWarehouseId,
                        Quantity = item.Quantity,
                        TransactionType = TransactionType.StockIn,
                        Remarks = $"توريد مشتريات بموجب فاتورة رقم {invoiceNumber}",
                        CreatedByUserId = userId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.PurchaseInvoiceItems.AddRangeAsync(purchaseItemsToSave);
                purchaseInvoice.TotalAmount = totalInvoiceAmount;

                // حفضة واحدة سريعة وآمنة لكل التغييرات
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new PurchaseInvoiceResponseDto
                {
                    IsSuccess = true,
                    Message = "تم تسجيل فاتورة المشتريات بنجاح وزيادة أرصدة المستودع وتحديث التكاليف.",
                    InvoiceId = purchaseInvoice.Id,
                    InvoiceNumber = purchaseInvoice.InvoiceNumber,
                    SupplierName = supplier.Name,
                    WarehouseName = warehouse?.Name ?? "غير معروف",
                    TotalAmount = purchaseInvoice.TotalAmount,
                    CreatedAt = purchaseInvoice.CreatedAt,
                    InvoiceItems = itemsReportForResponse
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return new PurchaseInvoiceResponseDto { IsSuccess = false, Message = "حدث خطأ غير متوقع أثناء معالجة فاتورة المشتريات." };
            }
        }

        public async Task<PurchaseInvoiceResponseDto?> GetPurchaseInvoiceById(
            int id, int? userWarehouseId, string userRole)
        {
            var query = _context.PurchaseInvoices
                .Where(pi => pi.Id == id)
                .AsNoTracking();

            if (userRole != "Admin" && userRole != "Manager" && userRole != "GeneralAccountant")
            {
                query = query.Where(pi => pi.WarehouseId == userWarehouseId);
            }

            return await query
                .Select(pi => new PurchaseInvoiceResponseDto
                {
                    IsSuccess = true,
                    InvoiceId = pi.Id,
                    InvoiceNumber = pi.InvoiceNumber,
                    SupplierName = pi.Supplier.Name,
                    WarehouseName = pi.Warehouse.Name,
                    TotalAmount = pi.TotalAmount,
                    CreatedAt = pi.CreatedAt,
                    InvoiceItems = pi.Items.Select(item => new PurchaseItemReportDto
                    {
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        CostPrice = item.CostPrice,
                        TotalPrice = item.TotalPrice
                    }).ToList()
                })
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<PurchaseInvoiceResponseDto>> GetWarehousePurchaseInvoices(
            int? userWarehouseId, string userRole)
        {
            var query = _context.PurchaseInvoices.AsNoTracking().AsQueryable();

            if (userRole != "Admin" && userRole != "Manager" && userRole != "GeneralAccountant")
            {
                query = query.Where(pi => pi.WarehouseId == userWarehouseId);
            }

            return await query
                .OrderByDescending(pi => pi.CreatedAt)
                .Select(pi => new PurchaseInvoiceResponseDto
                {
                    IsSuccess = true,
                    InvoiceId = pi.Id,
                    InvoiceNumber = pi.InvoiceNumber,
                    SupplierName = pi.Supplier.Name,
                    WarehouseName = pi.Warehouse.Name,
                    TotalAmount = pi.TotalAmount,
                    CreatedAt = pi.CreatedAt
                })
                .ToListAsync();
        }
    }
}
