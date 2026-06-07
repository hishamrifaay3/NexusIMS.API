using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexusIMS.API.DTOs.SalesInvoiceDTOs;
using NexusIMS.API.Services;
using System.Security.Claims;

namespace NexusIMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalesInvoiceController : ControllerBase
    {
        private readonly ISalesInvoiceService _invoiceService;

        public SalesInvoiceController(ISalesInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Cashier")] // الكاشير والإدارة هما اللي بيبيعوا
        public async Task<IActionResult> CreateInvoice([FromBody] InvoiceCreateDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // لقط البيانات الثلاثية الأمنية من الـ Token
            var cashierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Cashier";
            var warehouseClaim = User.FindFirst("WarehouseId")?.Value;

            int? cashierWarehouseId = string.IsNullOrEmpty(warehouseClaim) ? null : int.Parse(warehouseClaim);

            if (string.IsNullOrEmpty(cashierId)) return Unauthorized();

            var result = await _invoiceService.CreateInvoice(model, cashierId, cashierWarehouseId);

            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        // 2. تسجيل مستند مرتجع مبيعات احترافي (جزئي أو كلي)
        [HttpPost("return")]
        [Authorize(Roles = "Admin,Manager,Cashier")] // الكاشير يقدر يعمل مرتجع لزبون واقف قدامه
        public async Task<IActionResult> ExecuteReturn([FromBody] SalesReturnCreateDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Cashier";
            var warehouseClaim = User.FindFirst("WarehouseId")?.Value;

            int? userWarehouseId = string.IsNullOrEmpty(warehouseClaim) ? null : int.Parse(warehouseClaim);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _invoiceService.ExecuteSalesReturn(model, userId, userWarehouseId, role);

            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }


        // 3. جلب كل فواتير المبيعات (مفلترة تلقائياً بحارس الفصل التام)
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,GeneralAccountant,Cashier")]
        public async Task<IActionResult> GetAllInvoices()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Cashier";
            var warehouseClaim = User.FindFirst("WarehouseId")?.Value;

            int? warehouseId = string.IsNullOrEmpty(warehouseClaim) ? null : int.Parse(warehouseClaim);

            var invoices = await _invoiceService.GetWarehouseInvoices(warehouseId, role);
            return Ok(invoices);
        }



        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Manager,GeneralAccountant,Cashier")]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Cashier";
            var warehouseClaim = User.FindFirst("WarehouseId")?.Value;

            int? warehouseId = string.IsNullOrEmpty(warehouseClaim) ? null : int.Parse(warehouseClaim);

            var invoice = await _invoiceService.GetInvoiceById(id, warehouseId, role);
            if (invoice == null)
                return NotFound(new { Message = "الفاتورة غير موجودة أو غير مصرح لك بالوصول إليها." });

            return Ok(invoice);
        }
    }
}
