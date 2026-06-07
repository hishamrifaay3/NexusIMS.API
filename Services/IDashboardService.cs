using NexusIMS.API.DTOs.DashboardDTOs;

namespace NexusIMS.API.Services
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardData(
            DateTime? startDate, DateTime? endDate, int? userWarehouseId, string userRole);
    }
}
