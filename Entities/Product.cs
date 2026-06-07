using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        [Range(0,int.MaxValue,ErrorMessage = "السعر لا يمكن أن يكون بالسالب")]
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        public string CreatedByUserId { get; set; } = string.Empty;

        public virtual ApplicationUser CreatedByUser { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? DeletedByUserId { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
