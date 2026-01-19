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
    // 1️⃣ 新增商品（新商品 → 商品待審核）
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

        // ❗ 原始邏輯：只要賣場存在就能建商品
        // （不判斷 Status、不做商品審核）

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

            // ⭐ 關鍵：什麼都不管
            IsActive = null,     // 第一波商品
            Status = null,       // 不走商品審核
            CreatedAt = DateTime.Now
        };

        _db.StoreProducts.Add(product);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            product.ProductId,
            Message = "商品建立成功"
        });
    }

    // =========================================================
    // 2️⃣ 修改商品（修改後 → 商品重新審核）
    // =========================================================
    [HttpPut("{productId}")]
    public async Task<IActionResult> UpdateProduct(
        int storeId,
        int productId,
        [FromBody] UpdateStoreProductDto dto)
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

        // ① 只更新「允許修改」的欄位
        product.Price = dto.Price;
        product.Quantity = dto.Quantity;
        product.Description = dto.Description;
        product.EndDate = dto.EndDate;
        product.UpdatedAt = DateTime.Now;

        // ② 修改後 → 商品重新進入審核
        product.Status = 4;
        //product.IsActive = 0;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            product.ProductId,
            Message = "商品已更新，重新進入審核"
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
        var store = await _db.Stores
            .FirstOrDefaultAsync(s => s.StoreId == storeId);

        if (store == null)
            return NotFound("賣場不存在");

        var product = await _db.StoreProducts
            .FirstOrDefaultAsync(p => p.ProductId == productId
                                   && p.StoreId == storeId);

        if (product == null)
            return NotFound("商品不存在");

        // 軟刪除：不顯示 + 狀態下架
        product.Status = 0;
        //product.IsActive = 0;
        product.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
