using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusIMS.API.Data;
using NexusIMS.API.DTOs.CategoryDTOs;
using NexusIMS.API.Entities;

namespace NexusIMS.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryResponseDto> Create(CategoryDto model, string userId)
        {
            if (model == null)
                return new CategoryResponseDto 
                { 
                    IsSuccess = false, 
                    Message = "يجب إدخال بيانات صحيحة." 
                };

            var categoryExists = await _context.Categories.AnyAsync(c => c.Name == model.Name);
            if (categoryExists) 
                return new CategoryResponseDto
                { IsSuccess = false,
                    Message = "هذا القسم موجود بالفعل!" 
                };

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return new CategoryResponseDto 
            { IsSuccess = true,
                Message = "تم إضافة القسم بنجاح.",
                Name = category.Name, 
                Id = category.Id 
            };
        }

        public async Task<CategoryResponseDto> Update(CategoryDto model, string userId)
        {
            if (model == null || model.Id == null) 
                return new CategoryResponseDto
                {
                    IsSuccess = false,
                    Message = "بيانات التعديل غير كاملة." 
                };

            var category = await _context.Categories.FindAsync(model.Id);
            if (category == null) 
                return new CategoryResponseDto
                { 
                    IsSuccess = false,
                    Message = "هذا القسم غير موجود."
                };

            var nameExists = await _context.Categories.AnyAsync(c => c.Name == model.Name && c.Id != model.Id);
            if (nameExists) 
                return new CategoryResponseDto 
                { 
                    IsSuccess = false,
                    Message = "هناك قسم آخر مسجل بنفس هذا الاسم!" 
                };

            category.Name = model.Name;
            category.Description = model.Description;
            category.UpdatedByUserId = userId;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new CategoryResponseDto 
            { 
                IsSuccess = true,
                Message = "تم تعديل القسم بنجاح.", 
                Name = category.Name,
                Id = category.Id 
            };
        }

        public async Task<CategoryResponseDto> Delete(int Id)
        {
            var category = await _context.Categories.FindAsync(Id);
            if (category == null)
                return new CategoryResponseDto
                { 
                    IsSuccess = false, 
                    Message = "هذا القسم غير موجود."
                };

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return new CategoryResponseDto
                { 
                    IsSuccess = true,
                    Message = "تم حذف القسم بنجاح." 
                };
            }
            catch (DbUpdateException)
            {
                return new CategoryResponseDto 
                {
                    IsSuccess = false,
                    Message = "لا يمكن حذف هذا القسم لأنه يحتوي على منتجات مرتبطة به." 
                };
            }
        }

        public async Task<IEnumerable<CategoryListDto>> GetAll()
        {
            return await _context.Categories
                .AsNoTracking()
                .Select(c => new CategoryListDto { Id = c.Id, Name = c.Name, Description = c.Description })
                .ToListAsync();
        }
    }
}
