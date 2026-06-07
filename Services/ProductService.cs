using Microsoft.EntityFrameworkCore;
using NexusIMS.API.Data;
using NexusIMS.API.DTOs.ProductDTOs;
using NexusIMS.API.Entities;

namespace NexusIMS.API.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductResponseDto> CreateProduct(ProductCreateDto model, string userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return new ProductResponseDto
                {
                    IsSuccess = false,
                    Message = "جلسة العمل الحالية غير صالحة (ربما تم تصفير قاعدة البيانات). يرجى إعادة تسجيل الدخول."
                };
            }

            var category = await _context.Categories.FindAsync(model.CategoryId);
            if (category == null)
                return new ProductResponseDto { IsSuccess = false, Message = "القسم الذي أدخلته غير موجود." };

            var nameExists = await _context.Products.AnyAsync(p => p.Name == model.Name && !p.IsDeleted);
            if (nameExists)
                return new ProductResponseDto { IsSuccess = false, Message = "هذا المنتج مسجل بالفعل في النظام بنفس الاسم!" };

            var prefix = category.Name.Length >= 3 ? category.Name.Substring(0, 3).ToUpper() : category.Name.ToUpper();
            var productCount = await _context.Products.CountAsync(p => p.CategoryId == model.CategoryId);
            var sku = $"{prefix}-{(productCount + 1).ToString("D5")}";

            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                CategoryId = model.CategoryId,
                SKU = sku,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync(); 

            var allWarehouses = await _context.Warehouses.ToListAsync();
            foreach (var warehouse in allWarehouses)
            {
                var initialStock = new Stock
                {
                    ProductId = product.Id,
                    WarehouseId = warehouse.Id,
                    Quantity = 0
                };
                await _context.Stocks.AddAsync(initialStock);
            }
            await _context.SaveChangesAsync();

            return new ProductResponseDto
            {
                IsSuccess = true,
                Message = "تم إضافة المنتج بنجاح وتصفير أرصدته بالمخازن.",
                Name = model.Name,
                SKU = sku,
                Price = model.Price,
                CategoryName = category.Name
            };
        }

        public async Task<ProductResponseDto> UpdateProduct(int id, ProductUpdateDto model, string userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return new ProductResponseDto { IsSuccess = false, Message = "المستخدم الحالي غير صالح، يرجى إعادة تسجيل الدخول." };
            }

            var product = await _context.Products.Include(p => p.Category).SingleOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (product == null)
            {
                return new ProductResponseDto { IsSuccess = false, Message = "هذا المنتج غير موجود أو تم حذفه سابقاً." };
            }

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                var nameExists = await _context.Products.AnyAsync(p => p.Name == model.Name && p.Id != id && !p.IsDeleted);
                if (nameExists)
                {
                    return new ProductResponseDto { IsSuccess = false, Message = "هناك منتج آخر نشط يحمل نفس هذا الاسم!" };
                }
            }
            if (model.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == model.CategoryId.Value);
                if (!categoryExists)
                {
                    return new ProductResponseDto { IsSuccess = false, Message = "القسم المحدد غير موجود بالنظام." };
                }
            }

            model.UpdateEntity(product);

            product.UpdatedByUserId = userId;
            product.UpdatedAt = DateTime.UtcNow;


            await _context.SaveChangesAsync();


            var finalCategoryName = model.CategoryId.HasValue
                ? (await _context.Categories.FindAsync(product.CategoryId))?.Name
                : product.Category?.Name;

            return new ProductResponseDto
            {
                IsSuccess = true,
                Message = "تم تعديل بيانات المنتج بنجاح.",
                Name = product.Name,
                Price = product.Price, 
                SKU = product.SKU,
                CategoryName = finalCategoryName ?? "غير محدد"
            };
        }

        public async Task<ProductResponseDto> DeleteProduct(int id, string userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return new ProductResponseDto { IsSuccess = false, Message = "المستخدم الحالي غير صالح، يرجى إعادة تسجيل الدخول." };
            }

            var product = await _context.Products.SingleOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (product == null)
                return new ProductResponseDto { IsSuccess = false, Message = "هذا المنتج غير موجود أو تم حذفه بالفعل." };

            product.IsDeleted = true;
            product.DeletedByUserId = userId;
            product.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ProductResponseDto
            {
                IsSuccess = true,
                Message = "تم حذف المنتج بنجاح (Soft Delete) وحفظ بيانات المسؤول عن الحذف.",
                Name = product.Name,
                SKU = product.SKU
            };
        }

        public async Task<ProductListDto?> GetProductById(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.CreatedByUser)
                .Where(p => !p.IsDeleted)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    CreatedByUserName = p.CreatedByUser.FullName
                })
                .SingleOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<ProductListDto>> GetAllProducts()
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.CreatedByUser)
                .Where(p => !p.IsDeleted)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    CreatedByUserName = p.CreatedByUser.FullName
                }).ToListAsync();
        }
    }
}