namespace StoreApi.Dtos
{
    public class StoreReviewProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
