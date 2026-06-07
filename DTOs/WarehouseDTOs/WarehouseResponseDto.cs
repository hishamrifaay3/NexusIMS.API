namespace NexusIMS.API.DTOs.WarehouseDTOs
{
    public class WarehouseResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
