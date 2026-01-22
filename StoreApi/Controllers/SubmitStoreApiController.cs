using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Models;


namespace StoreApi.Controllers
{
    [ApiController]
    [Route("api/store")]
    public class StoreApiController : ControllerBase
    {
        private readonly StoreDbContext _db;

        public StoreApiController(StoreDbContext db)
        {
            _db = db;
        }

        //  賣家送審賣場
        [HttpPost("{storeId}/submit")]
        public async Task<IActionResult> SubmitStore(int storeId)
        {
            var store = await _db.Stores
                .Include(s => s.StoreProducts)
                .FirstOrDefaultAsync(s => s.StoreId == storeId);

            if (store == null)
                return NotFound("賣場不存在");

            if (store.Status != 0)
                return BadRequest("賣場狀態錯誤，無法送審");

            if (!store.StoreProducts.Any())
                return BadRequest("賣場至少需建立一個商品才能送審");

            store.Status = 1; //  審核中
            store.SubmittedAt = DateTime.Now;

            foreach (var product in store.StoreProducts)
            {
                if (product.Status == 0) // 尚未送審的第一波商品
                {
                    product.Status = 1;      // 商品審核中
                    product.IsActive = false; // 審核中前端不顯示
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "賣場已送審，內容已鎖定"
            });
        }
    }

}
