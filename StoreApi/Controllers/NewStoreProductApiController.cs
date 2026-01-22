using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Models;
using StoreApi.Dtos;
using StoreApi.Services;


namespace StoreApi.Controllers
{
    [ApiController]
    [Route("api/store/{storeId}/products/newproducts")]
 
    public class NewStoreProductApiController : ControllerBase
    {
        private readonly StoreDbContext _db;
        private readonly ImageUploadService _imageService;
        public NewStoreProductApiController(StoreDbContext db, ImageUploadService imageService)
        {
            _db = db;
            _imageService = imageService;
        }
        //  已發布賣場下新增商品（第二波）
        [HttpPost]
        public async Task<IActionResult> CreateNewProduct(int storeId,[FromForm] CreateStoreProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var store = await _db.Stores
                .FirstOrDefaultAsync(s => s.StoreId == storeId);

            if (store == null)
                return NotFound("賣場不存在");

            // 只能在已發布的賣場新增
            if (store.Status != 3)
                return BadRequest("僅限已發布賣場可新增商品");


         
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

                Status = 1,       // 新商品 -> 待審核
                IsActive = false, // 審核中不可於前端顯示
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
       
        // 已發布商品僅允許調整 價格跟數量（不送審）
    
        [HttpPut("{productId}/update-price-quantity")]
        public async Task<IActionResult> UpdateNewProduct(
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

            // 僅限已發布商品
            if (product.Status != 3)
                return BadRequest("商品尚未發布，無法使用此操作");

            // 驗證
            if (dto.Price < 0 || dto.Quantity < 0)
            {
                return BadRequest("價格或數量不可小於 0");
            }

            if (dto.Price > 50000 || dto.Quantity > 500)
            {
                return BadRequest("價格不可大於50000數量不可以大於500");
            }

            product.Price = dto.Price;
            product.Quantity = dto.Quantity;
            product.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "商品價格 / 數量已更新"
            });
        }

      
        // 修改商品名稱跟圖片則強制重新審核
        [HttpPut("{productId}/updatereview")]
        public async Task<IActionResult> UpdateNewProductReview(
            int storeId,
            int productId,
            [FromForm] UpdateNewProductReviewDto dto)
        {

            var product = await _db.StoreProducts
             .Include(p => p.Store)
             .FirstOrDefaultAsync(p =>
             p.ProductId == productId &&
             p.StoreId == storeId);

            if (product == null)
                return NotFound("商品不存在");

            if (product.Status != 3)
            {
                return BadRequest("只有已發布商品才能使用此操作");
            }

            if (string.IsNullOrWhiteSpace(dto.ProductName))
            {
                return BadRequest("商品名稱不可為空");
            }

            if (product.Store.Status == 4)
            {
                return BadRequest("賣場已停權，無法修改商品");
            }



            // ⭐ NEW 系列：存新圖
            var newImagePath = await _imageService.SaveProductImageAsync(dto.Image);

            // 更新名稱（一定會進審核）
            product.ProductName = dto.ProductName;

            if (newImagePath != null)
            {
                // 刪舊圖
                _imageService.DeleteImage(product.ImagePath);

                product.ImagePath = newImagePath;
            }

            // 一律重新進審核
            product.Status = 1;
            product.IsActive = false;
            product.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "商品已更新，重新進入審核"
            });
        }

   
        // 下架商品（不刪資料）
        [HttpDelete("{productId}/invisible")]
        public async Task<IActionResult> invisibleProduct(
            int storeId,
            int productId)
        {
            var product = await _db.StoreProducts
                .Include(p => p.Store)
                .FirstOrDefaultAsync(p =>
                    p.ProductId == productId &&
                    p.StoreId == storeId);

            if (product == null)
                return NotFound("商品不存在");

            if (product.Store.Status == 4)
                return BadRequest("賣場已停權，無法操作商品");

            if (!product.IsActive)
            {
                return BadRequest("商品已是下架狀態");
            }

            product.IsActive = false;
            product.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "商品已下架"
            });
        }

        // 重新上架商品
        [HttpPut("{productId}/visible")]
        public async Task<IActionResult> VisibleProduct(
            int storeId,
            int productId)
        {
            var product = await _db.StoreProducts
                .Include(p => p.Store)
                .FirstOrDefaultAsync(p =>
                    p.ProductId == productId &&
                    p.StoreId == storeId);

            if (product == null)
                return NotFound("商品不存在");

            // 只能上架已發布商品
            if (product.Status != 3)
                return BadRequest("商品尚未通過審核，無法上架");

            // 停權賣場不可上架
            if (product.Store.Status == 4)
                return BadRequest("賣場已停權，無法上架商品");

            if (product.IsActive)
                return BadRequest("商品已是上架狀態");

            product.IsActive = true;
            product.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "商品已重新上架"
            });
        }

    }
}
