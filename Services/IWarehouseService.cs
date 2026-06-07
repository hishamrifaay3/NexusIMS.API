using NexusIMS.API.DTOs.WarehouseDTOs;

namespace NexusIMS.API.Services
{
    public interface IWarehouseService
    {
        Task<WarehouseResponseDto> Create(WarehouseDto model);
        Task<WarehouseResponseDto> Update(WarehouseDto model);
        Task<WarehouseResponseDto> Delete(int id);
        Task<IEnumerable<WarehouseListDto>> GetAll();
    }
}
