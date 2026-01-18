using System;
using System.Collections.Generic;

namespace StoreApi.Models;

public partial class StoreOrder
{
    public int OrderId { get; set; }

    public int StoreId { get; set; }

    public int ProductId { get; set; }

    public string BuyerUid { get; set; } = null!;

    public decimal OrderPrice { get; set; }

    public int Quantity { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PlatformFeeRate { get; set; }

    public decimal PlatformFeeAmount { get; set; }

    public decimal SellerIncome { get; set; }

    public byte Status { get; set; }

    public bool IsSettled { get; set; }

    public DateTime? SettledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual StoreProduct Product { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;
}
