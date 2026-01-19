using System;
using System.Collections.Generic;

namespace StoreApi.Models;

public partial class Store
{
    public int StoreId { get; set; }

    public string SellerUid { get; set; } = null!;

    public string StoreName { get; set; } = null!;

    public byte Status { get; set; }

    public int ReviewFailCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsReported { get; set; }

    public int ReportCount { get; set; }

    public DateTime? LastReportedAt { get; set; }

    public virtual ICollection<StoreProduct> StoreProducts { get; set; } = new List<StoreProduct>();

    public virtual ICollection<StoreReview> StoreReviews { get; set; } = new List<StoreReview>();
}
