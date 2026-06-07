using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexusIMS.API.DTOs.CategoryDTOs;
using NexusIMS.API.Services;

namespace NexusIMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAll();
            return Ok(categories);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Storekeeper")]
        public async Task<IActionResult> Create([FromBody] CategoryDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _categoryService.Create(model, userId!);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = "Admin,Manager,Storekeeper")]
        public async Task<IActionResult> Update([FromBody] CategoryDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _categoryService.Update(model, userId!);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager,Storekeeper")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.Delete(id);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }


    }
}
