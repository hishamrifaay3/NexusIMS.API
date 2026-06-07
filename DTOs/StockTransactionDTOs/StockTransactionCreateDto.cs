using NexusIMS.API.Entities;
using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.DTOs.StockTransactionDTOs
{
    public class StockTransactionCreateDto
    {
        [Required(ErrorMessage = "يجب تحديد المنتج")]
        public int ProductId { get; set; }
        public int? WarehouseId { get; set; }

        [Required(ErrorMessage = "يجب إدخال الكمية")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية أكبر من صفر")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "يجب تحديد نوع الحركة")]
        public TransactionType TransactionType { get; set; }

        [StringLength(250, ErrorMessage = "السبب لا يزيد عن 250 حرف")]
        public string Remarks { get; set; } = string.Empty; 
    }
}
