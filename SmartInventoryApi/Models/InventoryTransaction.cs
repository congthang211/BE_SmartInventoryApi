using System;
using System.Collections.Generic;

namespace SmartInventoryApi.Models;

public partial class InventoryTransaction
{
    public int Id { get; set; }

    public string TransactionCode { get; set; } = null!;

    public int ProductId { get; set; }

    public int? SourceWarehouseId { get; set; }

    public int? DestinationWarehouseId { get; set; }

    public int Quantity { get; set; }

    public string TransactionType { get; set; } = null!;

    public int? ReferenceId { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Notes { get; set; }

    public int CreatedBy { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Warehouse? DestinationWarehouse { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Warehouse? SourceWarehouse { get; set; }
}
