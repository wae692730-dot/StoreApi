using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Models;
using StoreApi.Dtos;

namespace StoreApi.Controllers
{
    [ApiController]
    [Route("api/review/products")]
    public class NewProductReviewApiController : ControllerBase
    {
        private readonly StoreDbContext _db;

        public NewProductReviewApiController(StoreDbContext db)
        {
            _db = db;
        }

        // =========================================================
        // 1️⃣ 取得「第二波待審核商品清單」
        // =========================================================
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingNewProducts()
        {
            var products = await _db.StoreProducts
                .Include(p => p.Store)
                .Where(p =>
                    p.Status == 1 &&           // 商品待審核
                    p.Store.Status == 3        // 賣場已發布
                )
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.Price,
                    p.Quantity,
                    p.Description,
                    p.ImagePath,

                    StoreId = p.Store.StoreId,
                    StoreName = p.Store.StoreName,

                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(products);
        }

        // =========================================================
        // 2️⃣ 審核通過 → 商品發布
        // =========================================================
        [HttpPost("{productId}/approve")]
        public async Task<IActionResult> ApproveProduct(
            int productId,
            [FromBody] ReviewDto dto)
        {
            var product = await _db.StoreProducts.FindAsync(productId);
            if (product == null) return NotFound();

            product.Status = 3;
            product.IsActive = true;
            product.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return Ok("商品審核通過");
        }

        // =========================================================
        // 3️⃣ 審核不通過 → 商品退回
        // =========================================================
        [HttpPost("{productId}/reject")]
        public async Task<IActionResult> RejectProduct(
        int productId,
        [FromBody] ReviewDto dto)
        {
            var product = await _db.StoreProducts.FindAsync(productId);
            if (product == null) return NotFound();

            product.Status = 2;
            product.IsActive = false;
            product.RejectReason = dto.Comment;

            await _db.SaveChangesAsync();
            return Ok("商品審核未通過");
        }
    }
}
