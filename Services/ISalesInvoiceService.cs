using NexusIMS.API.DTOs.SalesInvoiceDTOs;

namespace NexusIMS.API.Services
{
    public interface ISalesInvoiceService
    {
        Task<InvoiceResponseDto> CreateInvoice(InvoiceCreateDto model,
            string cashierId, int? cashierWarehouseId);
        Task<IEnumerable<InvoiceResponseDto>> GetWarehouseInvoices(int? warehouseId, string userRole);
        Task<InvoiceResponseDto?> GetInvoiceById(int id, int? warehouseId, string userRole);

        Task<InvoiceResponseDto> ExecuteSalesReturn(SalesReturnCreateDto model, 
            string userId, int? userWarehouseId, string userRole);
    }
}
