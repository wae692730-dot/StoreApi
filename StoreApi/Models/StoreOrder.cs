using System;
using System.Collections.Generic;

namespace StoreApi.Models;

public partial class StoreOrder
{
    public int StoreOrderId { get; set; }

    public int StoreId { get; set; }

    public string BuyerUid { get; set; } = null!;

    public decimal PlatformFeeAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal SellerReceivableAmount { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual Store Store { get; set; } = null!;

    public virtual ICollection<StoreOrderDetail> StoreOrderDetails { get; set; } = new List<StoreOrderDetail>();
}
