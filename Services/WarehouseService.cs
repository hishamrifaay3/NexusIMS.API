using Microsoft.EntityFrameworkCore;
using NexusIMS.API.Data;
using NexusIMS.API.DTOs.WarehouseDTOs;
using NexusIMS.API.Entities;

namespace NexusIMS.API.Services
{
    public class WarehouseService:IWarehouseService
    {
        private readonly ApplicationDbContext _context;

        public WarehouseService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<WarehouseResponseDto> Create(WarehouseDto model)
        {
            if (model == null)
            {
                return new WarehouseResponseDto { IsSuccess = false, Message = "البيانات المبعوثة غير صحيحة." };
            }
            var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Name == model.Name);
            if (warehouseExists)
            {
                return new WarehouseResponseDto { IsSuccess = false, Message = "هذا المخزن مسجل بالفعل في النظام!" };
            }

            var warehouse = new Warehouse
            {
                Name = model.Name,
                Location = model.Location
            };

            await _context.Warehouses.AddAsync(warehouse);
            await _context.SaveChangesAsync();

            return new WarehouseResponseDto
            {
                IsSuccess = true,
                Message = "تم إنشاء المخزن بنجاح.",
                Id = warehouse.Id,
                Name = warehouse.Name
            };
        }

        public async Task<WarehouseResponseDto> Update(WarehouseDto model)
        {
            if (model == null || model.Id == null)
            {
                return new WarehouseResponseDto { IsSuccess = false, Message = "بيانات التعديل غير كاملة." };
            }

            var warehouse = await _context.Warehouses.SingleOrDefaultAsync(w => w.Id == model.Id);
            if (warehouse == null)
            {
                return new WarehouseResponseDto { IsSuccess = false, Message = "هذا المخزن غير موجود." };
            }
            var nameExists = await _context.Warehouses.AnyAsync(w => w.Name == model.Name && w.Id != model.Id);
            if (nameExists)
            {
                return new WarehouseResponseDto { IsSuccess = false, Message = "هذا الاسم مستخدم بالفعل في مخزن آخر!" };
            }

            warehouse.Name = model.Name;
            warehouse.Location = model.Location;

            await _context.SaveChangesAsync();

            return new WarehouseResponseDto
            {
                IsSuccess = true,
                Message = "تم تعديل بيانات المخزن بنجاح.",
                Id = warehouse.Id,
                Name = warehouse.Name
            };
        }
        public async Task<WarehouseResponseDto> Delete(int id)
        {
            var warehouse = await _context.Warehouses.SingleOrDefaultAsync(w => w.Id == id);
            if (warehouse == null)
            {
                return new WarehouseResponseDto { IsSuccess = false, Message = "هذا المخزن غير موجود." };
            }

            try
            {
                _context.Warehouses.Remove(warehouse);
                await _context.SaveChangesAsync();

                return new WarehouseResponseDto
                {
                    IsSuccess = true,
                    Message = "تم حذف المخزن بنجاح.",
                    Name = warehouse.Name,
                    Id = warehouse.Id
                };
            }
            catch (DbUpdateException)
            {
                return new WarehouseResponseDto
                {
                    IsSuccess = false,
                    Message = "لا يمكن حذف هذا المخزن لوجود موظفين، فواتير، أو حركات مخزنية مرتبطة به تاريخياً!"
                };
            }
            catch (Exception)
            {
                return new WarehouseResponseDto { IsSuccess = false, Message = "حدث خطأ غير متوقع أثناء الحذف." };
            }
        }
        public async Task<IEnumerable<WarehouseListDto>> GetAll()
        {
            return await _context.Warehouses
                .AsNoTracking()
                .Select(w => new WarehouseListDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Location = w.Location
                })
                .ToListAsync();
        }
    }
}
