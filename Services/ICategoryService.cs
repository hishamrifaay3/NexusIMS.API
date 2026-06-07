using NexusIMS.API.DTOs.CategoryDTOs;

namespace NexusIMS.API.Services
{
    public interface ICategoryService
    {
        Task<CategoryResponseDto> Create(CategoryDto model, string userId);
        Task<CategoryResponseDto> Update(CategoryDto model, string userId);
        Task<CategoryResponseDto> Delete(int Id);
        Task<IEnumerable<CategoryListDto>> GetAll();

    }
}
