using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pr3_Gordov.API.Models;

namespace Pr3_Gordov.API.Data;

public partial class Pr3_GordovContext : DbContext
{
    public Pr3_GordovContext()
    {
    }

    public Pr3_GordovContext(DbContextOptions<Pr3_GordovContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Clothing> Clothings { get; set; }

    public virtual DbSet<ClothingSize> ClothingSizes { get; set; }

    public virtual DbSet<ClothingType> ClothingTypes { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=Pr3_Gordov;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Clothing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clothing__3214EC077CB042A4");

            entity.ToTable("Clothing");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.ClothingType).WithMany(p => p.Clothings)
                .HasForeignKey(d => d.ClothingTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Clothing__Clothi__5629CD9C");
        });

        modelBuilder.Entity<ClothingSize>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clothing__3214EC07EFDD4EFB");

            entity.HasIndex(e => new { e.ClothingId, e.LetterSize, e.NumericSize }, "UQ_ClothingSize").IsUnique();

            entity.Property(e => e.LetterSize).HasMaxLength(10);

            entity.HasOne(d => d.Clothing).WithMany(p => p.ClothingSizes)
                .HasForeignKey(d => d.ClothingId)
                .HasConstraintName("FK__ClothingS__Cloth__5BE2A6F2");
        });

        modelBuilder.Entity<ClothingType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clothing__3214EC07D1DEDE7E");

            entity.HasIndex(e => e.Name, "UQ__Clothing__737584F60B792811").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Orders__3214EC075B6A258E");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("created");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__UserId__60A75C0F");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderIte__3214EC07DA4910D8");

            entity.HasIndex(e => new { e.OrderId, e.ClothingSizeId }, "UQ_OrderItem").IsUnique();

            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.ClothingSize).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ClothingSizeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__Cloth__693CA210");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderItem__Order__68487DD7");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07C21DDE08");

            entity.HasIndex(e => e.Login, "UQ__Users__5E55825B0773C6D4").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.DeliveryAddress).HasMaxLength(500);
            entity.Property(e => e.Login).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
