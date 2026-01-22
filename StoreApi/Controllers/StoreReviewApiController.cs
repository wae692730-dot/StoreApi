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
    
    [HttpGet("storepending")] // 第一波 撈賣場+第一波賞品資料
    public async Task<IActionResult> GetPendingStores()
    {
        var stores = await _db.Stores
            .Include(s => s.StoreProducts)
            .Where(s => s.Status == 1)
            .ToListAsync();

        var result = stores.Select(s => new StoreReviewListDto
        {
            StoreId = s.StoreId,
            SellerId = s.SellerUid,
            StoreName = s.StoreName,
            Status = s.Status,
            ReviewFailCount = s.ReviewFailCount,
            CreatedAt = s.CreatedAt,

            StoreProducts = s.StoreProducts.Select(p => new StoreReviewProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Price = p.Price,
                Quantity = p.Quantity
            }).ToList()
        }).ToList();

        return Ok(result);
    }


  
    [HttpPost("{storeId}/storeapprove")]  // 賣場審核通過
    public async Task<IActionResult> ApproveStore(int storeId, [FromBody] ReviewDto dto)
    {
        var store = await _db.Stores
             .Include(s => s.StoreProducts) //  一定要 Include 商品
             .FirstOrDefaultAsync(s => s.StoreId == storeId);

        if (store == null)
            return NotFound();

        // 賣場通過
        store.Status = 3;             // 已發布
        store.ReviewFailCount = 0;

        // 連同賣場一起啟用第一波商品
        foreach (var product in store.StoreProducts)
        {
            product.Status = 3;       // 已發布
            product.IsActive = true;  // 前端顯示
        }

        // 寫入審核紀錄
        _db.StoreReviews.Add(new StoreReview
        {
            StoreId = storeId,
            ReviewerUid = dto.ReviewerUid,
            Result = 1,
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "賣場審核通過，第一波商品已同步上架" });
    }



    
    [HttpPost("{storeId}/storereject")]// 賣場審核不通過
    public async Task<IActionResult> RejectStore(int storeId, [FromBody] ReviewDto dto)
    {
        var store = await _db.Stores.FindAsync(storeId);
        if (store == null) return NotFound();

        store.Status = 2;               // 審核失敗
        store.ReviewFailCount += 1;

        _db.StoreReviews.Add(new StoreReview
        {
            StoreId = storeId,
            ReviewerUid = dto.ReviewerUid,
            Result = 2,
            Comment = dto.Comment,
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync();
        return Ok();
    }
}
