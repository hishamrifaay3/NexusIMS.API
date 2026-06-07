using NexusIMS.API.DTOs.StockTransactionDTOs;

namespace NexusIMS.API.Services
{
    public interface IStockTransactionService
    {
        Task<StockTransactionResponseDto> CreateTransaction(StockTransactionCreateDto model, string userId, int? userWarehouseId, string userRole);
        Task<IEnumerable<StockTransactionListDto>> GetTransactions(int? userWarehouseId, string userRole);
        Task<StockTransactionListDto?> GetTransactionById(int id, int? userWarehouseId, string userRole);
    }
}
