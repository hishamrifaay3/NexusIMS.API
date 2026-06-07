using NexusIMS.API.DTOs.PurchaseInvoiceDTOs;

namespace NexusIMS.API.Services
{
    public interface IPurchaseInvoiceService
    {
        Task<PurchaseInvoiceResponseDto> CreatePurchaseInvoice
            (PurchaseInvoiceCreateDto model, string userId, int? userWarehouseId, string userRole);
        Task<PurchaseInvoiceResponseDto?> GetPurchaseInvoiceById
            (int id, int? userWarehouseId, string userRole);
        Task<IEnumerable<PurchaseInvoiceResponseDto>> GetWarehousePurchaseInvoices
            (int? userWarehouseId, string userRole);
    }
}
