using System.ComponentModel.DataAnnotations;

namespace StoreApi.Dtos
{
    public class UpdateNewProductReviewDto
    {
        public string ProductName { get; set; } = null!;

       
        [Required(ErrorMessage = "請上傳商品圖片")]
        public IFormFile? Image { get; set; }
    }
}
