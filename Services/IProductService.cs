using NexusIMS.API.DTOs.ProductDTOs;

namespace NexusIMS.API.Services
{
    public interface IProductService
    {
        Task<ProductResponseDto> CreateProduct(ProductCreateDto model, string userId);
        Task<ProductResponseDto> UpdateProduct(int id, ProductUpdateDto model, string userId); // محتاج DTO للتعديل
        Task<ProductResponseDto> DeleteProduct(int id, string userId);
        Task<ProductListDto?> GetProductById(int id);
        Task<IEnumerable<ProductListDto>> GetAllProducts();
    }
}
