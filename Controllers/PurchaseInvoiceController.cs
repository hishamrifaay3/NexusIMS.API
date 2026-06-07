using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexusIMS.API.DTOs.PurchaseInvoiceDTOs;
using NexusIMS.API.Services;
using System.Security.Claims;

namespace NexusIMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseInvoiceController : ControllerBase
    {
        private readonly IPurchaseInvoiceService _purchaseService;

        public PurchaseInvoiceController(IPurchaseInvoiceService purchaseService)
        {
            _purchaseService = purchaseService;
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Storekeeper")] 
        public async Task<IActionResult> CreateInvoice([FromBody] PurchaseInvoiceCreateDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Storekeeper";
            var warehouseClaim = User.FindFirst("WarehouseId")?.Value;

            int? userWarehouseId = string.IsNullOrEmpty(warehouseClaim) ? null : int.Parse(warehouseClaim);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _purchaseService.CreatePurchaseInvoice(model, userId, userWarehouseId, role);

            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Manager,GeneralAccountant,Storekeeper")]
        public async Task<IActionResult> GetAllInvoices()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Storekeeper";
            var warehouseClaim = User.FindFirst("WarehouseId")?.Value;

            int? userWarehouseId = string.IsNullOrEmpty(warehouseClaim) ? null : int.Parse(warehouseClaim);

            var invoices = await _purchaseService.GetWarehousePurchaseInvoices(userWarehouseId, role);
            return Ok(invoices);
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,GeneralAccountant,Storekeeper")]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Storekeeper";
            var warehouseClaim = User.FindFirst("WarehouseId")?.Value;

            int? userWarehouseId = string.IsNullOrEmpty(warehouseClaim) ? null : int.Parse(warehouseClaim);

            var invoice = await _purchaseService.GetPurchaseInvoiceById(id, userWarehouseId, role);
            if (invoice == null)
                return NotFound(new { Message = "الفاتورة غير موجودة أو غير مصرح لك برؤيتها." });

            return Ok(invoice);
        }
    }
}
