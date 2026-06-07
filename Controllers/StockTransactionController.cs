using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexusIMS.API.DTOs.StockTransactionDTOs;
using NexusIMS.API.Services;
using System.Security.Claims;

namespace NexusIMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StockTransactionController : ControllerBase
    {
        private readonly IStockTransactionService _transactionService;

        public StockTransactionController(IStockTransactionService transactionService)
        {
            _transactionService = transactionService;
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Manager,GeneralAccountant,Storekeeper")]
        public async Task<IActionResult> GetAll()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Cashier";
            var warehouseClaim = User.FindFirst("WarehouseId")?.Value;

            int? warehouseId = string.IsNullOrEmpty(warehouseClaim) ? null : int.Parse(warehouseClaim);

            var transactions = await _transactionService.GetTransactions(warehouseId, role);
            return Ok(transactions);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,GeneralAccountant,Storekeeper")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Cashier";
            var warehouseClaim = User.FindFirst("WarehouseId")?.Value;
            int? warehouseId = string.IsNullOrEmpty(warehouseClaim) ? null : int.Parse(warehouseClaim);

            var transaction = await _transactionService.GetTransactionById(id, warehouseId, role);
            if (transaction == null)
                return NotFound(new { Message = "الحركة المخزنية غير موجودة أو غير مصرح لك برؤيتها." });

            return Ok(transaction);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Storekeeper")] 
        public async Task<IActionResult> Create([FromBody] StockTransactionCreateDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Storekeeper";
            var warehouseClaim = User.FindFirst("WarehouseId")?.Value;

            int? warehouseId = string.IsNullOrEmpty(warehouseClaim) ? null : int.Parse(warehouseClaim);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _transactionService.CreateTransaction(model, userId, warehouseId, role);

            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }
    }
}
