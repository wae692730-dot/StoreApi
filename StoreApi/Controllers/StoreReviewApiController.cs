using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Models;
using StoreApi.Dtos;


[ApiController]
[Route("api/review")]
public class StoreReviewApiController : ControllerBase
{
    private readonly StoreDbContext _db;

    public StoreReviewApiController(StoreDbContext db)
    {
        _db = db;
    }

    // 4️⃣ 待審核商品
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingProducts()
    {
        var products = await _db.StoreProducts
            .Where(p => p.Status == 1) // 1 = 審核中
            .Include(p => p.Store)     // 可選：包含賣場資訊
            .ToListAsync();

        return Ok(products.Select(a => new Dictionary<string, object>
        {
            { "ProductId",a.ProductId },
            { "ProductName",a.ProductName },
            { "Price",a.Price },
        }).ToList());
    }

    // 5️⃣ 商品審核通過
    [HttpPost("product/{productId}/approve")]
    public async Task<IActionResult> ApproveProduct(int productId, [FromBody] ReviewDto dto)
    {
        var product = await _db.StoreProducts.FindAsync(productId);
        if (product == null) return NotFound("商品不存在");

        product.Status = 3;             // 已發布 (審核通過)
        product.ReviewFailCount = 0;    // 重置失敗次數

        _db.StoreReviews.Add(new StoreReview
        {
            ProductId = productId,     // 記錄商品ID
            ReviewerUid = dto.ReviewerUid,
            Result = 1,                // 1 = 通過 (假設 enum/convention)
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync();
        return Ok(new { Message = "商品審核通過" });
    }

    // 6️⃣ 商品審核不通過
    [HttpPost("product/{productId}/reject")]
    public async Task<IActionResult> RejectProduct(int productId, [FromBody] ReviewDto dto)
    {
        var product = await _db.StoreProducts.FindAsync(productId);
        if (product == null) return NotFound("商品不存在");

        product.Status = 2;               // 審核失敗
        product.ReviewFailCount += 1;

        _db.StoreReviews.Add(new StoreReview
        {
            ProductId = productId,
            ReviewerUid = dto.ReviewerUid,
            Result = 2,                   // 2 = 不通過
            Comment = dto.Comment,
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync();
        return Ok(new { Message = "商品審核不通過" });
    }
}
