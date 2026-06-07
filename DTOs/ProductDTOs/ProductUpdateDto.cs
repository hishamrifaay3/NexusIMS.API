using NexusIMS.API.Entities;
using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.DTOs.ProductDTOs
{
    public class ProductUpdateDto
    {
        [StringLength(150, ErrorMessage = "اسم المنتج لا يمكن أن يتخطى 150 حرف.")]
        public string? Name { get; set; } 

        [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون السعر أكبر من صفر")]
        public decimal? Price { get; set; } 

        public int? CategoryId { get; set; }



        public void UpdateEntity(Product product)
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                product.Name = Name;
            }
            if (Price.HasValue)
            {
                product.Price = Price.Value;
            }

            if (CategoryId.HasValue)
            {
                product.CategoryId = CategoryId.Value;
            }
        }
    }
}