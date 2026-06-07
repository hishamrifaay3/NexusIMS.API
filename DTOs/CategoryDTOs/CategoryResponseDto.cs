namespace NexusIMS.API.DTOs.CategoryDTOs
{
    public class CategoryResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Id { get; set; }
    }
}
