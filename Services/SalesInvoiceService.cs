using Microsoft.EntityFrameworkCore;
using NexusIMS.API.Data;
using NexusIMS.API.DTOs.SalesInvoiceDTOs;
using NexusIMS.API.Entities;

namespace NexusIMS.API.Services
{
    public class SalesInvoiceService : ISalesInvoiceService
    {
        private readonly ApplicationDbContext _context;

        public SalesInvoiceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceResponseDto> CreateInvoice(
            InvoiceCreateDto model, string cashierId, int? cashierWarehouseId)
        {
            if (cashierWarehouseId == null)
                return new InvoiceResponseDto
                {
                    IsSuccess = false,
                    Message = "عذراً، هذا الحساب غير مرتبط بمخزن حالياً، لا يمكن البيع."
                };

            if (model.Items == null || !model.Items.Any())
                return new InvoiceResponseDto { IsSuccess = false, Message = "لا يمكن حفظ فاتورة بدون أصناف." };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var validatedItems = new List<(InvoiceItemDto item, Product product, Stock stock)>();
                decimal totalInvoiceAmount = 0;

                foreach (var item in model.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null || product.IsDeleted)
                    {
                        await transaction.RollbackAsync();
                        return new InvoiceResponseDto
                        {
                            IsSuccess = false,
                            Message = $"المنتج ذو الرقم {item.ProductId} غير موجود في النظام."
                        };
                    }

                    var stock = await _context.Stocks
                        .SingleOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == cashierWarehouseId);

                    if (stock == null || stock.Quantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return new InvoiceResponseDto
                        {
                            IsSuccess = false,
                            Message = $"الرصيد لا يكفي للصنف ({product.Name}). المتاح حالياً في مخزنك هو {stock?.Quantity ?? 0} قطع فقط."
                        };
                    }

                    validatedItems.Add((item, product, stock));
                    totalInvoiceAmount += product.Price * item.Quantity;
                }

                var currentYear = DateTime.UtcNow.Year.ToString();
                var lastId = await _context.SalesInvoices.MaxAsync(i => (int?)i.Id) ?? 0;
                var invoiceNumber = $"INV-{currentYear}-{(lastId + 1).ToString("D4")}";

                var salesInvoice = new SalesInvoice
                {
                    InvoiceNumber = invoiceNumber,
                    WarehouseId = cashierWarehouseId.Value,
                    CreatedByUserId = cashierId,
                    CustomerName = model.CustomerName,
                    Remarks = model.Remarks,
                    TotalAmount = totalInvoiceAmount,
                    CreatedAt = DateTime.UtcNow,
                    Status = 1
                };

                await _context.SalesInvoices.AddAsync(salesInvoice);
                await _context.SaveChangesAsync();

                var invoiceItemsToSave = new List<InvoiceItem>();
                var itemsReportForResponse = new List<InvoiceItemReportDto>();

                decimal accumulatedDiscount = 0;
                decimal accumulatedTax = 0;
                decimal accumulatedFinalAmount = 0;

                foreach (var (item, product, stock) in validatedItems)
                {
                    stock.Quantity -= item.Quantity;

                    decimal itemTotal = product.Price * item.Quantity;

                    // 🌟 الحساب الاحترافي الموزع للسطر
                    decimal itemDiscount = itemTotal * (model.DiscountPercentage / 100);
                    decimal itemTax = (itemTotal - itemDiscount) * (model.TaxPercentage / 100);
                    decimal itemNetPrice = itemTotal - itemDiscount + itemTax;

                    accumulatedDiscount += itemDiscount;
                    accumulatedTax += itemTax;
                    accumulatedFinalAmount += itemNetPrice;

                    invoiceItemsToSave.Add(new InvoiceItem
                    {
                        SalesInvoiceId = salesInvoice.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        TotalPrice = itemTotal,
                        ItemDiscount = itemDiscount,
                        ItemTax = itemTax,
                        NetPrice = itemNetPrice // حفظ الصافي للسطر
                    });

                    itemsReportForResponse.Add(new InvoiceItemReportDto
                    {
                        ProductName = product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        TotalPrice = itemNetPrice // إرجاع الصافي في التقرير ليكون معبراً عن القيمة الفعلية
                    });
                }

                await _context.InvoiceItems.AddRangeAsync(invoiceItemsToSave);

                // ربط رأس الفاتورة بمجاميع السطور الدقيقة
                salesInvoice.Discount = accumulatedDiscount;
                salesInvoice.Tax = accumulatedTax;
                salesInvoice.FinalAmount = accumulatedFinalAmount;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new InvoiceResponseDto
                {
                    IsSuccess = true,
                    Message = "تم إصدار الفاتورة بنجاح والخصم من المخزن المربوط.",
                    InvoiceId = salesInvoice.Id,
                    InvoiceNumber = salesInvoice.InvoiceNumber,
                    CustomerName = salesInvoice.CustomerName,
                    TotalAmount = salesInvoice.TotalAmount,
                    Discount = salesInvoice.Discount,
                    Tax = salesInvoice.Tax,
                    FinalAmount = salesInvoice.FinalAmount,
                    CreatedAt = salesInvoice.CreatedAt,
                    InvoiceItems = itemsReportForResponse
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return new InvoiceResponseDto
                {
                    IsSuccess = false,
                    Message = "حدث خطأ غير متوقع أثناء معالجة الفاتورة."
                };
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetWarehouseInvoices(int? warehouseId, string userRole)
        {
            var query = _context.SalesInvoices
                .Include(i => i.Items)
                .AsNoTracking()
                .AsQueryable();

            if (userRole != "Admin" && userRole != "Manager" && userRole != "GeneralAccountant")
            {
                query = query.Where(i => i.WarehouseId == warehouseId);
            }

            return await query
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new InvoiceResponseDto
                {
                    IsSuccess = true,
                    InvoiceId = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    CustomerName = i.CustomerName,
                    TotalAmount = i.TotalAmount,
                    Discount = i.Discount,
                    Tax = i.Tax,
                    FinalAmount = i.FinalAmount,
                    CreatedAt = i.CreatedAt,
                    InvoiceItems = i.Items.Select(item => new InvoiceItemReportDto
                    {
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<InvoiceResponseDto?> GetInvoiceById(int id, int? warehouseId, string userRole)
        {
            var query = _context.SalesInvoices
                .Include(i => i.Items)
                .ThenInclude(item => item.Product)
                .Where(i => i.Id == id)
                .AsNoTracking()
                .AsQueryable();

            if (userRole != "Admin" && userRole != "Manager" && userRole != "GeneralAccountant")
            {
                query = query.Where(i => i.WarehouseId == warehouseId);
            }

            var invoice = await query.SingleOrDefaultAsync();
            if (invoice == null) return null;

            return new InvoiceResponseDto
            {
                IsSuccess = true,
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerName = invoice.CustomerName,
                TotalAmount = invoice.TotalAmount,
                Discount = invoice.Discount,
                Tax = invoice.Tax,
                FinalAmount = invoice.FinalAmount,
                CreatedAt = invoice.CreatedAt,
                InvoiceItems = invoice.Items.Select(item => new InvoiceItemReportDto
                {
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList()
            };
        }

        public async Task<InvoiceResponseDto> ExecuteSalesReturn(SalesReturnCreateDto model,
    string userId, int? userWarehouseId, string userRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var invoice = await _context.SalesInvoices
                    .Include(i => i.Items)
                    .SingleOrDefaultAsync(i => i.Id == model.SalesInvoiceId);

                if (invoice == null)
                    return new InvoiceResponseDto
                    {
                        IsSuccess = false,
                        Message = "الفاتورة الأصلية غير موجودة."
                    };

                if (userRole != "Admin" && userRole != "Manager" && userRole != "GeneralAccountant")
                {
                    if (invoice.WarehouseId != userWarehouseId)
                        return new InvoiceResponseDto
                        {
                            IsSuccess = false,
                            Message = "غير مصرح لك بعمل مرتجع لفاتورة تابعة لمخزن آخر."
                        };
                }

                if (invoice.Status == 3)
                    return new InvoiceResponseDto
                    {
                        IsSuccess = false,
                        Message = "هذه الفاتورة تم إرجاعها بالكامل سابقاً!"
                    };

                var previouslyReturnedItems = await _context.SalesReturnItems
                    .Where(ri => ri.SalesReturn.SalesInvoiceId == model.SalesInvoiceId)
                    .GroupBy(ri => ri.ProductId)
                    .Select(g => new { ProductId = g.Key, TotalReturned = g.Sum(x => x.Quantity) })
                    .ToListAsync();

                // الـ Validation المسبق الشامل
                foreach (var item in model.ItemsToReturn)
                {
                    var originalItem = invoice.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
                    if (originalItem == null)
                    {
                        await transaction.RollbackAsync();
                        return new InvoiceResponseDto
                        {
                            IsSuccess = false,
                            Message = "هذا المنتج لا ينتمي للفاتورة الأصلية."
                        };
                    }

                    var pastReturnedQty = previouslyReturnedItems
                        .FirstOrDefault(p => p.ProductId == item.ProductId)?.TotalReturned ?? 0;
                    var allowedReturnQty = originalItem.Quantity - pastReturnedQty;

                    if (item.Quantity > allowedReturnQty)
                    {
                        await transaction.RollbackAsync();
                        return new InvoiceResponseDto
                        {
                            IsSuccess = false,
                            Message = $"الكمية المطلوبة أكبر من المسموح بإرجاعه. المتبقي القابل للإرجاع هو {allowedReturnQty} قطع."
                        };
                    }
                }

                // توليد رقم مستند المرتجع
                var returnCount = await _context.SalesReturns.CountAsync();
                var returnNumber = $"RET-{DateTime.UtcNow.Year}-{(returnCount + 1).ToString("D4")}";

                var salesReturn = new SalesReturn
                {
                    SalesInvoiceId = invoice.Id,
                    WarehouseId = invoice.WarehouseId,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Remarks = model.Remarks,
                    TotalReturnAmount = 0
                };

                await _context.SalesReturns.AddAsync(salesReturn);
                await _context.SaveChangesAsync();

                decimal totalReturnAmount = 0;
                var returnItemsToSave = new List<SalesReturnItem>();

                foreach (var item in model.ItemsToReturn)
                {
                    var originalItem = invoice.Items.First(i => i.ProductId == item.ProductId);

                    var stock = await _context.Stocks
                        .SingleOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == invoice.WarehouseId);
                    if (stock != null) stock.Quantity += item.Quantity;

                    // 🌟 الحساب الاحترافي الصافي الدقيق بناءً على الخانات الجديدة:
                    decimal actualNetUnitPrice = originalItem.NetPrice / originalItem.Quantity; // صافي سعر القطعة بعد الخصم والضريبة
                    decimal itemReturnPrice = actualNetUnitPrice * item.Quantity; // إجمالي المسترد الفعلي لهذه الكمية
                    totalReturnAmount += itemReturnPrice;

                    returnItemsToSave.Add(new SalesReturnItem
                    {
                        SalesReturnId = salesReturn.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = originalItem.UnitPrice,
                        TotalPrice = itemReturnPrice
                    });

                    await _context.StockTransactions.AddAsync(new StockTransaction
                    {
                        ProductId = item.ProductId,
                        WarehouseId = invoice.WarehouseId,
                        Quantity = item.Quantity,
                        TransactionType = TransactionType.StockIn,
                        Remarks = $"مرتجع تلقائي بموجب مستند {returnNumber}",
                        CreatedByUserId = userId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SalesReturnItems.AddRangeAsync(returnItemsToSave);
                salesReturn.TotalReturnAmount = totalReturnAmount;

                invoice.UpdatedByUserId = userId;
                invoice.UpdatedAt = DateTime.UtcNow;

                // حساب الـ Status بناءً على مجموع الكميات الفعلي لكل صنف
                bool isFullReturn = invoice.Items.All(originalItem =>
                {
                    var pastReturned = previouslyReturnedItems
                        .FirstOrDefault(p => p.ProductId == originalItem.ProductId)?.TotalReturned ?? 0;
                    var currentReturned = model.ItemsToReturn
                        .FirstOrDefault(i => i.ProductId == originalItem.ProductId)?.Quantity ?? 0;
                    return (pastReturned + currentReturned) >= originalItem.Quantity;
                });

                invoice.Status = isFullReturn ? 3 : 2;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new InvoiceResponseDto
                {
                    IsSuccess = true,
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    CustomerName = invoice.CustomerName,
                    TotalAmount = invoice.TotalAmount,
                    Discount = invoice.Discount,
                    Tax = invoice.Tax,
                    FinalAmount = invoice.FinalAmount,
                    Message = $"تم تسجيل مستند المرتجع {returnNumber} بنجاح. المبلغ المسترد الصافي للعميل كاش: {totalReturnAmount:N2}، وتم تحديث حالة الفاتورة وإعادة البضائع للمخازن."
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return new InvoiceResponseDto { IsSuccess = false, Message = "حدث خطأ غير متوقع أثناء معالجة مستند المرتجع." };
            }
        }
    }
}