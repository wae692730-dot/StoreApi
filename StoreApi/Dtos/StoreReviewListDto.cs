namespace StoreApi.Dtos
{
    public class StoreReviewListDto
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // 底下的商品
        public List<StoreReviewProductDto> Products { get; set; } = new();
    }

}
