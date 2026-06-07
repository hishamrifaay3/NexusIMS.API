using NexusIMS.API.Data;
using NexusIMS.API.DTOs.StockTransactionDTOs;
using NexusIMS.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace NexusIMS.API.Services
{
    public class StockTransactionService : IStockTransactionService
    {
        private readonly ApplicationDbContext _context;

        public StockTransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StockTransactionResponseDto> CreateTransaction(StockTransactionCreateDto model,
            string userId,int? userWarehouseId,string userRole)
        {

            int targetWarehouseId = -1;

            if (userRole == "Admin" || userRole == "Manager")
            {
                if (model.WarehouseId == null)
                    return new StockTransactionResponseDto
                    {
                        IsSuccess = false,
                        Message = "يجب ادخال المخزن"
                    };
                targetWarehouseId = model.WarehouseId.Value;
            }
            else
            {
                // Storekeeper
                if (userWarehouseId == null)
                    return new StockTransactionResponseDto
                    {
                        IsSuccess = false,
                        Message = "يجب ادخال المخزن"
                    };
                if (model.WarehouseId != null && model.WarehouseId != userWarehouseId)
                    return new StockTransactionResponseDto
                    {
                        IsSuccess = false,
                        Message = "غير مصرح لك بالتعامل مع هذا المخزن"
                    };

                targetWarehouseId = userWarehouseId.Value;
            }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null || product.IsDeleted)
                return new StockTransactionResponseDto
                {
                    IsSuccess = false,
                    Message = "هذا المنتج غير موجود او تم حذفه"
                };

            var stock = await _context.Stocks
                .SingleOrDefaultAsync(s =>
                    s.ProductId == model.ProductId &&
                    s.WarehouseId == targetWarehouseId);

            if (stock == null)
                return new StockTransactionResponseDto
                {
                    IsSuccess = false,
                    Message = "يوجد خطأ في اسم المنتج او المخزن"
                };

            switch (model.TransactionType)
            {
                case TransactionType.StockIn:
                    stock.Quantity += model.Quantity;
                    break;

                case TransactionType.StockOut:
                case TransactionType.Damage:
                    if (stock.Quantity < model.Quantity)
                        return new StockTransactionResponseDto
                        {
                            IsSuccess = false,
                            Message = "الكمية الحالية في المخزن لا تكفي لإتمام هذه العملية!",
                            ProductQuantity = stock.Quantity
                        };
                    stock.Quantity -= model.Quantity;
                    break;

                default:
                    return new StockTransactionResponseDto
                    {
                        IsSuccess = false,
                        Message = "نوع الحركة المخزنية غير مدعوم!"
                    };
            }

            var transaction = new StockTransaction
            {
                ProductId = model.ProductId,
                WarehouseId = targetWarehouseId,
                Quantity = model.Quantity,
                TransactionType = model.TransactionType,
                Remarks = model.Remarks,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.StockTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            return new StockTransactionResponseDto
            {
                IsSuccess = true,
                Message = "تمت العمليه بنجاح",
                TransactionType = model.TransactionType.ToString(),
                ProductId = model.ProductId,
                WarehouseId = targetWarehouseId,
                TransactionQuantity = model.Quantity,
                ProductQuantity = stock.Quantity
            };
        }

        public async Task<IEnumerable<StockTransactionListDto>> GetTransactions(
            int? userWarehouseId,string userRole)
        {
            var query = _context.StockTransactions
                .AsNoTracking()
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .Include(t => t.CreatedByUser)
                .AsQueryable();

            if (userRole != "Admin" && userRole != "Manager" && userRole != "GeneralAccountant")
            {
                query = query.Where(t => t.WarehouseId == userWarehouseId);
            }

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new StockTransactionListDto
                {
                    Id = t.Id,
                    ProductName = t.Product.Name,
                    SKU = t.Product.SKU,
                    WarehouseName = t.Warehouse.Name,
                    Quantity = t.Quantity,
                    TransactionType = t.TransactionType.ToString(),
                    Remarks = t.Remarks,
                    CreatedByUserName = t.CreatedByUser.FullName,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<StockTransactionListDto?> GetTransactionById(
            int id,int? userWarehouseId,string userRole)
        {
            var query = _context.StockTransactions
                .AsNoTracking()
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .Include(t => t.CreatedByUser)
                .Where(t => t.Id == id);

            if (userRole != "Admin" && userRole != "Manager" && userRole != "GeneralAccountant")
            {
                query = query.Where(t => t.WarehouseId == userWarehouseId);
            }

            return await query
                .Select(t => new StockTransactionListDto
                {
                    Id = t.Id,
                    ProductName = t.Product.Name,
                    SKU = t.Product.SKU,
                    WarehouseName = t.Warehouse.Name,
                    Quantity = t.Quantity,
                    TransactionType = t.TransactionType.ToString(),
                    Remarks = t.Remarks,
                    CreatedByUserName = t.CreatedByUser.FullName,
                    CreatedAt = t.CreatedAt
                })
                .SingleOrDefaultAsync();
        }
    }
}