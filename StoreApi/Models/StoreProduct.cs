using System;
using System.Collections.Generic;

namespace StoreApi.Models;

public partial class StoreProduct
{
    public int ProductId { get; set; }

    public int StoreId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public string? Location { get; set; }

    public string? ImagePath { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte Status { get; set; }

    public int ReviewFailCount { get; set; }

    public virtual Store Store { get; set; } = null!;
}
