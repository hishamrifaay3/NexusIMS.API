using System.ComponentModel.DataAnnotations;

namespace NexusIMS.API.Entities
{
    public class Stock
    {
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = default!;
        public int WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set; } = default!;
        [Range(0, int.MaxValue, ErrorMessage = "الكمية لا يمكن أن تكون بالسالب")]
        public int Quantity { get; set; }


        
    }
}
