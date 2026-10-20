using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HardwareInventoryTracker_App.Models;

public partial class HardwareInventoryManagerContext : DbContext
{
    public HardwareInventoryManagerContext()
    {
    }

    public HardwareInventoryManagerContext(DbContextOptions<HardwareInventoryManagerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<HardwareItem> HardwareItems { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<OrderHistory> OrderHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=HardwareInventoryManager;User Id=HardwareManager;Password=Hardware123!;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.Property(e => e.CategoryName).HasMaxLength(50);
            entity.Property(e => e.CategoryType).HasMaxLength(50);
        });

        modelBuilder.Entity<HardwareItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Inventory");

            entity.ToTable("HardwareItem");

            entity.Property(e => e.CategoryId).HasColumnName("Category_Id");
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.HardwareItems)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HardwareItem_Category");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Inventory1");

            entity.ToTable("Inventory");

            entity.Property(e => e.HardwareItemId).HasColumnName("HardwareItem_Id");

            entity.HasOne(d => d.HardwareItem).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.HardwareItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inventory_HardwareItem");
        });

        modelBuilder.Entity<OrderHistory>(entity =>
        {
            entity.ToTable("OrderHistory");

            entity.Property(e => e.HardwareItemId).HasColumnName("HardwareItem_Id");
            entity.Property(e => e.OrderPrice).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.OrderTotal).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.HardwareItem).WithMany(p => p.OrderHistories)
                .HasForeignKey(d => d.HardwareItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderHistory_HardwareItem");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
