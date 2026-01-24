using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Models;
using StoreApi.Dtos;

[ApiController]
[Route("api/buyer/orders")]
public class BuyerOrderApiController : ControllerBase
{
    private readonly StoreDbContext _db;

    public BuyerOrderApiController(StoreDbContext db)
    {
        _db = db;
    }
  
    [HttpPost] // 建立買家訂單
    public async Task<IActionResult> CreateBuyerOrder(
        [FromBody] CreateBuyerOrderDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            return BadRequest("訂單必須包含至少一項商品");

        // 驗證賣場
        var store = await _db.Stores
            .FirstOrDefaultAsync(s => s.StoreId == dto.StoreId && s.Status == 3);

        if (store == null)
            return BadRequest("賣場不存在或尚未發布");

        // 驗證商品 + 計算金額
        decimal totalAmount = 0;
        var orderItems = new List<BuyerOrderDetail>();

        foreach (var item in dto.Items)
        {
            var product = await _db.StoreProducts
                .FirstOrDefaultAsync(p =>
                    p.ProductId == item.StoreProductId &&
                    p.StoreId == dto.StoreId &&
                    p.Status == 3);

            if (product == null)
                return BadRequest($"商品 {item.StoreProductId} 不存在或不可販售");

            if (product.Quantity < item.Quantity)
                return BadRequest($"商品 {product.ProductName} 庫存不足");

            var subtotal = product.Price * item.Quantity;
            totalAmount += subtotal;

            orderItems.Add(new BuyerOrderDetail
            {
                StoreProductId = product.ProductId,
                ProductName = product.ProductName,
                UnitPrice = product.Price,
                Quantity = item.Quantity,
                SubtotalAmount = subtotal
            });

            // 預扣庫存（稍後 SaveChanges）
            product.Quantity -= item.Quantity;
        }

        // 建立訂單主檔
        var order = new BuyerOrder
        {
            BuyerUid = dto.BuyerUid, // 之後可改成從 JWT 取
            StoreId = dto.StoreId,
            TotalAmount = totalAmount,

            ReceiverName = dto.ReceiverName,
            ReceiverPhone = dto.ReceiverPhone,
            ShippingAddress = dto.ShippingAddress,

            Status = 1, // 已成立（準備中）
            CreatedAt = DateTime.Now
        };

        _db.BuyerOrders.Add(order);
        await _db.SaveChangesAsync(); // 先拿到 buyer_order_id

        // 建立訂單明細
        foreach (var item in orderItems)
        {
            item.BuyerOrderId = order.BuyerOrderId;
            _db.BuyerOrderDetails.Add(item);
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "訂單建立成功",
            orderId = order.BuyerOrderId,
            totalAmount = order.TotalAmount
        });
    }
}
