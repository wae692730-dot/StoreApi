namespace StoreApi.Dtos
{
    public class UpdateStoreProductDto
    {
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public string ProductName { get; set; } = null!;
        public DateTime? EndDate { get; set; }

    }
}
