using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.DTOs.CategoryDTOs
{
    public class CategoryDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "يجب ادخال اسم القسم")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "يجب ادخال وصف القسم")]
        public string Description { get; set; } = string.Empty;
    }
}
