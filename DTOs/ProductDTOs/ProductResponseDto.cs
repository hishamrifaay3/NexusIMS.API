namespace NexusIMS.API.DTOs.ProductDTOs
{
    public class ProductResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
