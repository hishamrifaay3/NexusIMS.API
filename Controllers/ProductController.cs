using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexusIMS.API.DTOs.ProductDTOs;
using NexusIMS.API.Services;
using System.Security.Claims;

namespace NexusIMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProducts();
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null) return NotFound(new { Message = "هذا المنتج غير موجود!" });

            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Storekeeper")]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(string.IsNullOrEmpty(userId)) 
                return Unauthorized(new { Message = "فشل في التعرف على هوية المستخدم الحالية." });
            var result = await _productService.CreateProduct(model, userId);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager,Storekeeper")] 
        public async Task<IActionResult> Update(int id ,[FromBody] ProductUpdateDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _productService.UpdateProduct(id,model, userId);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager,Storekeeper")] 
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _productService.DeleteProduct(id, userId);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }
    }
}
