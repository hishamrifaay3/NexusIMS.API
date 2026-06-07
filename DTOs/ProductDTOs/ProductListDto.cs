namespace NexusIMS.API.DTOs.ProductDTOs
{
    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CreatedByUserName { get; set; } = string.Empty;
    }
}
