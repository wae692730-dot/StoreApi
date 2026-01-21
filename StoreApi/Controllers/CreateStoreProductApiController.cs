using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Models;
using StoreApi.Dtos;
using StoreApi.Services;


namespace StoreApi.Controllers;

[ApiController]
[Route("api/store/{storeId}/products")]

public class CreateStoreProductApiController : ControllerBase
{
    private readonly StoreDbContext _db;
    private readonly ImageUploadService _imageService;



    public CreateStoreProductApiController(StoreDbContext db, ImageUploadService imageService)
    {
        _db = db;
        _imageService = imageService;
    }
  
    [HttpPost]//  建立第一波商品（商品隨賣場一起進審核）
    public async Task<IActionResult> CreateProduct(int storeId,[FromForm] CreateStoreProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var store = await _db.Stores
            .FirstOrDefaultAsync(s => s.StoreId == storeId);

        if (store == null)
            return NotFound("賣場不存在");

        // 只有草稿可新增商品
        if (store.Status != 0)
        {
            return BadRequest(new
            {
                message = "賣場已送審或已發布，禁止再新增商品"
            });
        }

        var imagePath = await _imageService.SaveProductImageAsync(dto.Image);

        var product = new StoreProduct
        {
            StoreId = storeId,
            ProductName = dto.ProductName,
            Description = dto.Description,
            Price = dto.Price,
            Quantity = dto.Quantity,
            Location = dto.Location,
            EndDate = dto.EndDate,

            // ⭐ 圖片重點
            ImagePath = imagePath,

            Status = 1,
            CreatedAt = DateTime.Now
        };

        _db.StoreProducts.Add(product);
        await _db.SaveChangesAsync();
        return Ok(new
        {
            product.ProductId,
            product.ImagePath,
            Message = "商品已建立，隨賣場進入審核"
        });
    }

    [HttpPut("{productId}/Edit")]   // 修改審核中的商品資訊
    public async Task<IActionResult> EditProduct(int storeId,int productId,[FromForm] EditProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _db.StoreProducts
            .FirstOrDefaultAsync(p => p.ProductId == productId
                                   && p.StoreId == storeId);

        if (product == null)
            return NotFound("商品不存在");

        
        if (product.Store.Status == 3) 
            return BadRequest("賣場已發布，請使用商品異動 API");

        bool needReReview = false;

        //  風控欄位（可直接改，不進審核）
        if (product.Price != dto.Price)
            product.Price = dto.Price;
        if (product.Quantity != dto.Quantity)
            product.Quantity = dto.Quantity;

        // 重大欄位（改了就要進審核）
        if (product.ProductName != dto.ProductName)
        {
            product.ProductName = dto.ProductName;
            needReReview = true;
        }
        
        var newImagePath = await _imageService.SaveProductImageAsync(dto.Image);

        if (newImagePath != null)
        {
            _imageService.DeleteImage(product.ImagePath);

            product.ImagePath = newImagePath;
            needReReview = true;
        }


        // 其他可自由調整的資訊
        product.Description = dto.Description;
        product.EndDate = dto.EndDate;
        product.UpdatedAt = DateTime.Now;

        // 若有重大變更則重新進審核
        if (needReReview)
        {
            product.Status = 1; // 待審核
            product.IsActive = false; // 資料庫顯示0 前端不會看到
        }


        await _db.SaveChangesAsync();

        return Ok(new
        {
            product.ProductId,
            Message = needReReview
                ? "商品已更新，因關鍵資訊變更重新進入審核"
                : "商品已更新（價格 / 數量調整）"
        });
    }


    //  刪除商品按鈕但資料庫還是會存在紀錄 讓狀態變成0 變成草稿中可新增賞品
    [HttpDelete("{productId}")]
    public async Task<IActionResult> DeleteProduct(
        int storeId,
        int productId)
    {
        var store = await _db.Stores.FindAsync(storeId);
        if (store == null)
            return NotFound("賣場不存在");

        var product = await _db.StoreProducts
            .FirstOrDefaultAsync(p => p.ProductId == productId
                                   && p.StoreId == storeId);

        if (product == null)
            return NotFound("商品不存在");

        // 下架
        product.IsActive = false;
        product.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        // 純提示文字 沒任何功能
        if (store.Status == 0 || store.Status == 1)
        {
            return Ok(new
            {
                message = "商品尚未送審，可繼續新增商品並重新送審"
            });
        }

        return Ok(new
        {
            message = "商品已下架"
        });
    }
}
