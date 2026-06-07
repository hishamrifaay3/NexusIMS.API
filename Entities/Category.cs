namespace NexusIMS.API.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
