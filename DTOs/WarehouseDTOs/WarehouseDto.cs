using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.DTOs.WarehouseDTOs
{
    public class WarehouseDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "يجب إدخال اسم المخزن")]
        [StringLength(100, ErrorMessage = "اسم المخزن لا يزيد عن 100 حرف")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "يجب إدخال عنوان أو موقع المخزن")]
        [StringLength(200, ErrorMessage = "العنوان لا يزيد عن 200 حرف")]
        public string Location { get; set; } = string.Empty;
    }
}
