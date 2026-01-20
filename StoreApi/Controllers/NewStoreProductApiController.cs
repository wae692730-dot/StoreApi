using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Models;
using StoreApi.Dtos;
using StoreApi.Services;


namespace StoreApi.Controllers
{
    [ApiController]
    [Route("api/store/{storeId}/products/new")]
 
    public class NewStoreProductApiController : ControllerBase
    {
        private readonly StoreDbContext _db;
        private readonly ImageUploadService _imageService;


        public NewStoreProductApiController(StoreDbContext db, ImageUploadService imageService)
        {
            _db = db;
            _imageService = imageService;
        }

        // =========================================================
        // 1️⃣ 已發布賣場 → 新增商品（第二波）
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> CreateNewProduct(
            int storeId,
            [FromForm] CreateStoreProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var store = await _db.Stores
                .FirstOrDefaultAsync(s => s.StoreId == storeId);

            if (store == null)
                return NotFound("賣場不存在");

            // ❗ 只能在「已發布」賣場新增
            if (store.Status != 3)
                return BadRequest("僅限已發布賣場可新增商品");


            // ================== 圖片處理 ==================
            // ⭐ NEW 系列：用 Service 存圖
            var imagePath = await _imageService.SaveProductImageAsync(dto.Image);

            var product = new StoreProduct
            {
                StoreId = storeId,
                ProductName = dto.ProductName,
                Price = dto.Price,
                Quantity = dto.Quantity,
                Description = dto.Description,
                EndDate = dto.EndDate,
                Location = dto.Location,

                ImagePath = imagePath,

                Status = 1,       // 新商品 → 待審核
                IsActive = false, // 審核前不可顯示
                CreatedAt = DateTime.Now
            };

            _db.StoreProducts.Add(product);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "商品已建立，等待審核",
                product.ProductId,
                product.ImagePath
            });
        }
        // =========================================================
        // 2️⃣ 已發布商品 → 僅允許調整「價格 / 數量」（不進審）
        // =========================================================
        [HttpPut("{productId}/safe-update")]
        public async Task<IActionResult> UpdateNewProductDto(
            int storeId,
            int productId,
            [FromBody] UpdateNewProductDto dto)
        {
            var product = await _db.StoreProducts
                .FirstOrDefaultAsync(p =>
                    p.ProductId == productId &&
                    p.StoreId == storeId);

            if (product == null)
                return NotFound("商品不存在");

            // ❗ 僅限已發布商品
            if (product.Status != 3)
                return BadRequest("商品尚未發布，無法使用此操作");

            product.Price = dto.Price;
            product.Quantity = dto.Quantity;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "商品價格 / 數量已更新"
            });
        }

        // =========================================================
        // 3️⃣ 修改商品名稱 / 圖片 → 強制重新審核
        // =========================================================
        [HttpPut("{productId}/review-update")]
        public async Task<IActionResult> UpdateNewProductReview(
            int storeId,
            int productId,
            [FromForm] UpdateNewProductReviewDto dto)
        {


            var product = await _db.StoreProducts
                .FirstOrDefaultAsync(p =>
                    p.ProductId == productId &&
                    p.StoreId == storeId);

            if (product == null)
                return NotFound("商品不存在");

            // ================== 圖片處理 ==================
            // ⭐ NEW 系列：存新圖
            var newImagePath = await _imageService.SaveProductImageAsync(dto.Image);

            // 更新名稱（一定會進審核）
            product.ProductName = dto.ProductName;

            if (newImagePath != null)
            {
                // 👉 刪舊圖（決策在 Controller）
                _imageService.DeleteImage(product.ImagePath);

                product.ImagePath = newImagePath;
            }

            // ⭐ NEW：一律重新進審核
            product.Status = 1;
            product.IsActive = false;
            product.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "商品已更新，重新進入審核"
            });
        }

        // =========================================================
        // 4️⃣ 下架商品（不刪資料）
        // =========================================================
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DisableProduct(
            int storeId,
            int productId)
        {
            var product = await _db.StoreProducts
                .FirstOrDefaultAsync(p =>
                    p.ProductId == productId &&
                    p.StoreId == storeId);

            if (product == null)
                return NotFound("商品不存在");

            product.IsActive = false;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "商品已下架"
            });
        }
    }
}
