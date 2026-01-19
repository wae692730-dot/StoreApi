using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StoreApi.Models;

public partial class StoreDbContext : DbContext
{
    public StoreDbContext()
    {
    }

    public StoreDbContext(DbContextOptions<StoreDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<StoreProduct> StoreProducts { get; set; }

    public virtual DbSet<StoreReview> StoreReviews { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\sqlexpress;Database=StoreDb;Integrated Security=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.StoreId).HasName("PK__Store__A2F2A30C48FE1917");

            entity.ToTable("Store");

            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.LastReportedAt).HasColumnType("datetime");
            entity.Property(e => e.ReviewFailCount).HasColumnName("review_fail_count");
            entity.Property(e => e.SellerUid)
                .HasMaxLength(50)
                .HasColumnName("seller_uid");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.StoreName)
                .HasMaxLength(100)
                .HasColumnName("store_name");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<StoreProduct>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__StorePro__47027DF574809336");

            entity.ToTable("StoreProduct");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("image_path");
            entity.Property(e => e.IsActive).HasDefaultValue(1);
            entity.Property(e => e.LastReportedAt).HasColumnType("datetime");
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .HasColumnName("location");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Status).HasDefaultValue(4);
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Store).WithMany(p => p.StoreProducts)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreProduct_Store");
        });

        modelBuilder.Entity<StoreReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__StoreRev__60883D908F908BAB");

            entity.ToTable("StoreReview");

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Result).HasColumnName("result");
            entity.Property(e => e.ReviewerUid)
                .HasMaxLength(50)
                .HasColumnName("reviewer_uid");
            entity.Property(e => e.StoreId).HasColumnName("store_id");

            entity.HasOne(d => d.Store).WithMany(p => p.StoreReviews)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreReview_Store");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
