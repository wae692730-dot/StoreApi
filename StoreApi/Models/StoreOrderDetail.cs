using System;
using System.Collections.Generic;

namespace StoreApi.Models;

public partial class StoreOrderDetail
{
    public int StoreOrderDetailId { get; set; }

    public int StoreOrderId { get; set; }

    public int StoreProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal SubtotalAmount { get; set; }

    public virtual StoreOrder StoreOrder { get; set; } = null!;

    public virtual StoreProduct StoreProduct { get; set; } = null!;
}
