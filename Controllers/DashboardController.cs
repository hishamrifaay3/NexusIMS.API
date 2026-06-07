using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusIMS.API.Services;
using System.Security.Claims;

namespace NexusIMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,GeneralAccountant,Cashier,Storekeeper")] // كل الأدوار تقدر تفتح الـ Dashboard وتشوف داتا صلاحياتها
        public async Task<IActionResult> GetDashboard([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Cashier";
            var warehouseClaim = User.FindFirst("WarehouseId")?.Value;

            int? userWarehouseId = string.IsNullOrEmpty(warehouseClaim) ? null : int.Parse(warehouseClaim);

            var data = await _dashboardService.GetDashboardData(startDate, endDate, userWarehouseId, role);
            return Ok(data);
        }
    }
}