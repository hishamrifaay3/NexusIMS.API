using Microsoft.AspNetCore.Identity;

namespace NexusIMS.API.Entities
{
    public class ApplicationUser:IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public int? WarehouseId { get; set; }
        public virtual Warehouse? Warehouse { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
