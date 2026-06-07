using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusIMS.API.Data;
using NexusIMS.API.DTOs.SupplierDTOs;
using NexusIMS.API.Entities;

namespace NexusIMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SupplierController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SupplierController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. جلب كل الموردين
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,GeneralAccountant,Storekeeper")]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _context.Suppliers
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            return Ok(suppliers);
        }

        // 2. جلب مورد معين بالـ ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,GeneralAccountant")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound(new { Message = "المورد غير موجود." });
            return Ok(supplier);
        }

        // 3. إضافة مورد جديد
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([FromBody] SupplierCreateDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var supplier = new Supplier
            {
                Name = model.Name,
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                TaxNumber = model.TaxNumber,
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "تم تسجيل المورد بنجاح", SupplierId = supplier.Id });
        }

        // 4. تعديل بيانات مورد
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]

        public async Task<IActionResult> Update(int id, [FromBody] SupplierUpdateDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound(new { Message = "المورد غير موجود." });
            model.UpdateEntity(supplier);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "تم تحديث بيانات المورد بنجاح." });
        }
    }
}
