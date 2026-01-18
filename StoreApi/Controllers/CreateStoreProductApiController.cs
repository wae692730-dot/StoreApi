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

    // =========================================================
    // 1️⃣ 新增商品
    // =========================================================
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

        if (store.Status == 2)
            return BadRequest("審核失敗的賣場不可新增商品");

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
            CreatedAt = DateTime.Now
        };

        _db.StoreProducts.Add(product);

        // 已發布 → 修改後退回審核
        if (store.Status == 3)
            store.Status = 1;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            product.ProductId,
            Message = "商品建立成功"
        });
    }

    // 更新商品
    [HttpPut("{productId}")]
    public async Task<IActionResult> UpdateProduct(
        int storeId,
        int productId,
        [FromBody] StoreProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var store = await _db.Stores
            .FirstOrDefaultAsync(s => s.StoreId == storeId);

        if (store == null)
            return NotFound("賣場不存在");

        var product = await _db.StoreProducts
            .FirstOrDefaultAsync(p => p.ProductId == productId
                                   && p.StoreId == storeId);

        if (product == null)
            return NotFound("商品不存在");

        // ① 更新商品
        product.ProductName = dto.ProductName;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Quantity = dto.Quantity;
        product.Location = dto.Location;
        product.ImagePath = dto.ImagePath;
        product.EndDate = dto.EndDate;
        product.UpdatedAt = DateTime.Now;

        // ② 已發布 → 強制退回審核
        if (store.Status == 3)
        {
            store.Status = 1;

            // ⭐ 關鍵：明確標記 Store 為 Modified
            _db.Stores.Update(store);
        }

        // ③ 一次性儲存
        await _db.SaveChangesAsync();

        return Ok(new
        {
            product.ProductId,
            StoreStatus = store.Status,
            Message = "商品更新成功，賣場已退回審核"
        });
    }


    // =========================================================
    // 3️⃣ 刪除商品
    // =========================================================
    [HttpDelete("{productId}")]
    public async Task<IActionResult> DeleteProduct(
        int storeId,
        int productId)
    {
        var store = await _db.Stores
            .FirstOrDefaultAsync(s => s.StoreId == storeId);

        if (store == null)
            return NotFound("賣場不存在");

        var product = await _db.StoreProducts
            .FirstOrDefaultAsync(p => p.ProductId == productId
                                   && p.StoreId == storeId);

        if (product == null)
            return NotFound("商品不存在");

        if (store.Status == 2)
            return BadRequest("審核失敗的賣場不可刪除商品");

        _db.StoreProducts.Remove(product);

        // 已發布 → 刪除後退回審核
        if (store.Status == 3)
            store.Status = 1;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
