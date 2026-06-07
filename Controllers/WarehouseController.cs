using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexusIMS.API.DTOs.WarehouseDTOs;
using NexusIMS.API.Services;

namespace NexusIMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var warhouses = await _warehouseService.GetAll();
            return Ok(warhouses);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Create(WarehouseDto model)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _warehouseService.Create(model);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update([FromBody] WarehouseDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _warehouseService.Update(model);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _warehouseService.Delete(id);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }
    }
}
