using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Models;
using StoreApi.Dtos;

namespace StoreApi.Controllers;

[ApiController]
[Route("api/store/{storeId}/products")]
public class CreateStoreProductApiController : ControllerBase
{
    private readonly StoreDbContext _db;

    public CreateStoreProductApiController(StoreDbContext db)
    {
        _db = db;
    }

    //  建立第一波商品（商品隨賣場一起進審核）
  
    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        int storeId,
        [FromBody] CreateStoreProductDto dto)
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

        var product = new StoreProduct
        {
            StoreId = storeId,
            ProductName = dto.ProductName,
            Description = dto.Description,
            Price = dto.Price,
            Quantity = dto.Quantity,
            Location = dto.Location,
            ImagePath = dto.ImagePath,
            EndDate = dto.EndDate,

            Status = 1,               //  商品待審核（跟賣場一起）
            CreatedAt = DateTime.Now
        };

        _db.StoreProducts.Add(product);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            product.ProductId,
            Message = "商品已建立，隨賣場進入審核"
        });
    }

  
  //修改商品（風控更新 / 重大修改需重新審核）
   
    [HttpPut("{productId}")]
    public async Task<IActionResult> UpdateProduct(
        int storeId,
        int productId,
        [FromBody] UpdateStoreProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _db.StoreProducts
            .FirstOrDefaultAsync(p => p.ProductId == productId
                                   && p.StoreId == storeId);

        if (product == null)
            return NotFound("商品不存在");

        bool needReReview = false;

        // ========= 風控欄位（可直接改，不進審核） =========
        if (product.Price != dto.Price)
            product.Price = dto.Price;

        if (product.Quantity != dto.Quantity)
            product.Quantity = dto.Quantity;

        // ========= 重大欄位（改了就要進審核） =========
        if (product.ProductName != dto.ProductName)
        {
            product.ProductName = dto.ProductName;
            needReReview = true;
        }

        if (product.ImagePath != dto.ImagePath)
        {
            product.ImagePath = dto.ImagePath;
            needReReview = true;
        }

        // 其他可自由調整的資訊
        product.Description = dto.Description;
        product.EndDate = dto.EndDate;
        product.UpdatedAt = DateTime.Now;

        // 🔁 若有重大變更 → 重新進審核
        if (needReReview)
        {
            product.Status = 1; // 待審核
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

    // =========================================================
    // 3️⃣ 刪除商品（軟刪除：下架）
    // =========================================================
    [HttpDelete("{productId}")]
    public async Task<IActionResult> DeleteProduct(
        int storeId,
        int productId)
    {
        var product = await _db.StoreProducts
            .FirstOrDefaultAsync(p => p.ProductId == productId
                                   && p.StoreId == storeId);

        if (product == null)
            return NotFound("商品不存在");

        product.Status = 0;          // 下架
        product.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
