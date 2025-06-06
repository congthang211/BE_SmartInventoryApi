﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartInventoryApi.Models;

#nullable disable

namespace SmartInventoryApi.Migrations
{
    [DbContext(typeof(InventoryManagementDbContext))]
    partial class InventoryManagementDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SmartInventoryApi.Models.ActivityLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EntityId")
                        .HasColumnType("int");

                    b.Property<string>("EntityType")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Ipaddress")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("IPAddress");

                    b.Property<DateTime>("LogDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("Module")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PK__Activity__3214EC07E55F8F1D");

                    b.HasIndex("UserId");

                    b.ToTable("ActivityLogs");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int?>("ParentCategoryId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PK__Categori__3214EC07681BAE5B");

                    b.HasIndex("ParentCategoryId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Inventory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("LastUpdated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<int>("WarehouseId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PK__Inventor__3214EC079A65BF95");

                    b.HasIndex("WarehouseId");

                    b.HasIndex(new[] { "ProductId", "WarehouseId" }, "UQ_Inventory_Product_Warehouse")
                        .IsUnique();

                    b.ToTable("Inventory", (string)null);
                });

            modelBuilder.Entity("SmartInventoryApi.Models.InventoryTransaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CreatedBy")
                        .HasColumnType("int");

                    b.Property<int?>("DestinationWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<int?>("ReferenceId")
                        .HasColumnType("int");

                    b.Property<int?>("SourceWarehouseId")
                        .HasColumnType("int");

                    b.Property<string>("TransactionCode")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("TransactionDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("TransactionType")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id")
                        .HasName("PK__Inventor__3214EC07E12199A9");

                    b.HasIndex("CreatedBy");

                    b.HasIndex("DestinationWarehouseId");

                    b.HasIndex("ProductId");

                    b.HasIndex("SourceWarehouseId");

                    b.ToTable("InventoryTransactions");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("CreatedBy")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<DateTime?>("DeliveryDate")
                        .HasColumnType("datetime");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("OrderDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("OrderType")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int>("PartnerId")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasDefaultValue("Pending");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<int>("WarehouseId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PK__Orders__3214EC0756044F21");

                    b.HasIndex("CreatedBy");

                    b.HasIndex("PartnerId");

                    b.HasIndex("WarehouseId");

                    b.HasIndex(new[] { "Code" }, "UQ__Orders__A25C5AA7B9C0BABB")
                        .IsUnique();

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.OrderDetail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<int>("ProcessedQuantity")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("decimal(18, 2)");

                    b.HasKey("Id")
                        .HasName("PK__OrderDet__3214EC070E203DA5");

                    b.HasIndex("OrderId");

                    b.HasIndex("ProductId");

                    b.ToTable("OrderDetails");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Partner", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("ContactPerson")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Phone")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("TaxCode")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id")
                        .HasName("PK__Partners__3214EC077E30B28D");

                    b.ToTable("Partners");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Barcode")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal>("CostPrice")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<int?>("CreatedBy")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<int?>("DefaultSupplierId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageUrl")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<int?>("MaximumStock")
                        .HasColumnType("int");

                    b.Property<int>("MinimumStock")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<decimal>("SellingPrice")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id")
                        .HasName("PK__Products__3214EC07BF5511B4");

                    b.HasIndex("CategoryId");

                    b.HasIndex("CreatedBy");

                    b.HasIndex("DefaultSupplierId");

                    b.HasIndex(new[] { "Code" }, "UQ__Products__A25C5AA7CE10CFED")
                        .IsUnique();

                    b.ToTable("Products");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.StockTake", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("CreatedBy")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime");

                    b.Property<string>("Status")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasDefaultValue("Draft");

                    b.Property<int>("WarehouseId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PK__StockTak__3214EC078636DC8A");

                    b.HasIndex("CreatedBy");

                    b.HasIndex("WarehouseId");

                    b.HasIndex(new[] { "Code" }, "UQ__StockTak__A25C5AA72BC7477A")
                        .IsUnique();

                    b.ToTable("StockTakes");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.StockTakeDetail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CountedQuantity")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("StockTakeId")
                        .HasColumnType("int");

                    b.Property<int>("SystemQuantity")
                        .HasColumnType("int");

                    b.Property<int?>("Variance")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PK__StockTak__3214EC07A7D2D69B");

                    b.HasIndex("ProductId");

                    b.HasIndex("StockTakeId");

                    b.ToTable("StockTakeDetails");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<DateTime?>("LastLogin")
                        .HasColumnType("datetime");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Phone")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("UserRole")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasDefaultValue("Staff");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id")
                        .HasName("PK__Users__3214EC0703AE5B32");

                    b.HasIndex(new[] { "Username" }, "UQ__Users__536C85E415430959")
                        .IsUnique();

                    b.HasIndex(new[] { "Email" }, "UQ__Users__A9D1053446020B2E")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Warehouse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<string>("Location")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id")
                        .HasName("PK__Warehous__3214EC075781C825");

                    b.ToTable("Warehouses");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.ActivityLog", b =>
                {
                    b.HasOne("SmartInventoryApi.Models.User", "User")
                        .WithMany("ActivityLogs")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("FK_ActivityLogs_Users");

                    b.Navigation("User");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Category", b =>
                {
                    b.HasOne("SmartInventoryApi.Models.Category", "ParentCategory")
                        .WithMany("InverseParentCategory")
                        .HasForeignKey("ParentCategoryId")
                        .HasConstraintName("FK_Categories_ParentCategory");

                    b.Navigation("ParentCategory");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Inventory", b =>
                {
                    b.HasOne("SmartInventoryApi.Models.Product", "Product")
                        .WithMany("Inventories")
                        .HasForeignKey("ProductId")
                        .IsRequired()
                        .HasConstraintName("FK_Inventory_Products");

                    b.HasOne("SmartInventoryApi.Models.Warehouse", "Warehouse")
                        .WithMany("Inventories")
                        .HasForeignKey("WarehouseId")
                        .IsRequired()
                        .HasConstraintName("FK_Inventory_Warehouses");

                    b.Navigation("Product");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.InventoryTransaction", b =>
                {
                    b.HasOne("SmartInventoryApi.Models.User", "CreatedByNavigation")
                        .WithMany("InventoryTransactions")
                        .HasForeignKey("CreatedBy")
                        .IsRequired()
                        .HasConstraintName("FK_InventoryTransactions_Users");

                    b.HasOne("SmartInventoryApi.Models.Warehouse", "DestinationWarehouse")
                        .WithMany("InventoryTransactionDestinationWarehouses")
                        .HasForeignKey("DestinationWarehouseId")
                        .HasConstraintName("FK_InventoryTransactions_DestinationWarehouse");

                    b.HasOne("SmartInventoryApi.Models.Product", "Product")
                        .WithMany("InventoryTransactions")
                        .HasForeignKey("ProductId")
                        .IsRequired()
                        .HasConstraintName("FK_InventoryTransactions_Products");

                    b.HasOne("SmartInventoryApi.Models.Warehouse", "SourceWarehouse")
                        .WithMany("InventoryTransactionSourceWarehouses")
                        .HasForeignKey("SourceWarehouseId")
                        .HasConstraintName("FK_InventoryTransactions_SourceWarehouse");

                    b.Navigation("CreatedByNavigation");

                    b.Navigation("DestinationWarehouse");

                    b.Navigation("Product");

                    b.Navigation("SourceWarehouse");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Order", b =>
                {
                    b.HasOne("SmartInventoryApi.Models.User", "CreatedByNavigation")
                        .WithMany("Orders")
                        .HasForeignKey("CreatedBy")
                        .IsRequired()
                        .HasConstraintName("FK_Orders_Users");

                    b.HasOne("SmartInventoryApi.Models.Partner", "Partner")
                        .WithMany("Orders")
                        .HasForeignKey("PartnerId")
                        .IsRequired()
                        .HasConstraintName("FK_Orders_Partners");

                    b.HasOne("SmartInventoryApi.Models.Warehouse", "Warehouse")
                        .WithMany("Orders")
                        .HasForeignKey("WarehouseId")
                        .IsRequired()
                        .HasConstraintName("FK_Orders_Warehouses");

                    b.Navigation("CreatedByNavigation");

                    b.Navigation("Partner");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.OrderDetail", b =>
                {
                    b.HasOne("SmartInventoryApi.Models.Order", "Order")
                        .WithMany("OrderDetails")
                        .HasForeignKey("OrderId")
                        .IsRequired()
                        .HasConstraintName("FK_OrderDetails_Orders");

                    b.HasOne("SmartInventoryApi.Models.Product", "Product")
                        .WithMany("OrderDetails")
                        .HasForeignKey("ProductId")
                        .IsRequired()
                        .HasConstraintName("FK_OrderDetails_Products");

                    b.Navigation("Order");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Product", b =>
                {
                    b.HasOne("SmartInventoryApi.Models.Category", "Category")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId")
                        .IsRequired()
                        .HasConstraintName("FK_Products_Categories");

                    b.HasOne("SmartInventoryApi.Models.User", "CreatedByNavigation")
                        .WithMany("Products")
                        .HasForeignKey("CreatedBy")
                        .HasConstraintName("FK_Products_Users");

                    b.HasOne("SmartInventoryApi.Models.Partner", "DefaultSupplier")
                        .WithMany("Products")
                        .HasForeignKey("DefaultSupplierId")
                        .HasConstraintName("FK_Products_Partners");

                    b.Navigation("Category");

                    b.Navigation("CreatedByNavigation");

                    b.Navigation("DefaultSupplier");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.StockTake", b =>
                {
                    b.HasOne("SmartInventoryApi.Models.User", "CreatedByNavigation")
                        .WithMany("StockTakes")
                        .HasForeignKey("CreatedBy")
                        .IsRequired()
                        .HasConstraintName("FK_StockTakes_Users");

                    b.HasOne("SmartInventoryApi.Models.Warehouse", "Warehouse")
                        .WithMany("StockTakes")
                        .HasForeignKey("WarehouseId")
                        .IsRequired()
                        .HasConstraintName("FK_StockTakes_Warehouses");

                    b.Navigation("CreatedByNavigation");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.StockTakeDetail", b =>
                {
                    b.HasOne("SmartInventoryApi.Models.Product", "Product")
                        .WithMany("StockTakeDetails")
                        .HasForeignKey("ProductId")
                        .IsRequired()
                        .HasConstraintName("FK_StockTakeDetails_Products");

                    b.HasOne("SmartInventoryApi.Models.StockTake", "StockTake")
                        .WithMany("StockTakeDetails")
                        .HasForeignKey("StockTakeId")
                        .IsRequired()
                        .HasConstraintName("FK_StockTakeDetails_StockTakes");

                    b.Navigation("Product");

                    b.Navigation("StockTake");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Category", b =>
                {
                    b.Navigation("InverseParentCategory");

                    b.Navigation("Products");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Order", b =>
                {
                    b.Navigation("OrderDetails");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Partner", b =>
                {
                    b.Navigation("Orders");

                    b.Navigation("Products");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Product", b =>
                {
                    b.Navigation("Inventories");

                    b.Navigation("InventoryTransactions");

                    b.Navigation("OrderDetails");

                    b.Navigation("StockTakeDetails");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.StockTake", b =>
                {
                    b.Navigation("StockTakeDetails");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.User", b =>
                {
                    b.Navigation("ActivityLogs");

                    b.Navigation("InventoryTransactions");

                    b.Navigation("Orders");

                    b.Navigation("Products");

                    b.Navigation("StockTakes");
                });

            modelBuilder.Entity("SmartInventoryApi.Models.Warehouse", b =>
                {
                    b.Navigation("Inventories");

                    b.Navigation("InventoryTransactionDestinationWarehouses");

                    b.Navigation("InventoryTransactionSourceWarehouses");

                    b.Navigation("Orders");

                    b.Navigation("StockTakes");
                });
#pragma warning restore 612, 618
        }
    }
}
