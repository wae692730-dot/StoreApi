using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Models;
using StoreApi.Dtos;

[ApiController]
[Route("api/createstore")]

public class StoreApiController : ControllerBase
{
    private readonly StoreDbContext _db;

    public StoreApiController(StoreDbContext db)
    {
        _db = db;
    }

    
    [HttpPost] //  建立賣場
    public async Task<IActionResult> CreateStore([FromBody] CreateStoreDto dto)
    {
        var store = new Store
        {
            SellerUid = dto.SellerUid,
            StoreName = dto.StoreName,
            Status = 0,               // 草稿
            ReviewFailCount = 0,
            CreatedAt = DateTime.Now
        };
        // 計算此賣家已建立的賣場數量
        int storeCount = await _db.Stores
            .CountAsync(s => s.SellerUid == dto.SellerUid);

        // 若已達上限（10 個）則拒絕
        if (storeCount >= 10)
        {
            return BadRequest(new
            {
                message = "此賣家最多只能建立  10 個賣場"
            });
        }

        _db.Stores.Add(store);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            store.StoreId,
            store.Status
        });
    }

    
    [HttpGet("my/{sellerUid}/mystore")]//  賣家查看自己的賣場
    public async Task<IActionResult> GetMyStore(string sellerUid)
    {
        var stores = await _db.Stores
            .Where(s => s.SellerUid == sellerUid)
            .ToListAsync();

        return Ok(stores);
    }

   
    [HttpGet("forpublic")]   // 非會員對象可以查看賣場底下與商品
    public async Task<IActionResult> GetPublicStores()
    {
        var stores = await _db.Stores
      .Where(s => s.Status == 3) // 已發布賣場
      .Select(s => new
      {
          s.StoreId,
          s.StoreName,

          Products = s.StoreProducts
              .Where(p => p.Status == 3 && p.IsActive)
              .Select(p => new
              {
                  p.ProductId,
                  p.ProductName,
                  p.Price
              })
              .ToList()
      })
      .ToListAsync();

        return Ok(stores);
    }

 
    // 取得賣場詳細資料（包含底下所有商品）
    [HttpGet("{storeId}/myproduct")]
    public async Task<IActionResult> GetStoreDetail(int storeId)
    {
        var store = await _db.Stores
            .Where(s => s.StoreId == storeId)
            .Select(s => new
            {
                s.StoreId,
                s.StoreName,
                s.Status,
                s.ReviewFailCount,
                s.CreatedAt,

                Products = s.StoreProducts
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new
                    {
                        p.ProductId,
                        p.ProductName,
                        p.Price,
                        p.Quantity,
                        p.Status,
                        p.IsActive,
                        p.CreatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (store == null)
            return NotFound("賣場不存在");

        return Ok(store);
    }

}
