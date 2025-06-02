using System;
using System.Collections.Generic;

namespace SmartInventoryApi.Models;

public partial class StockTake
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public int WarehouseId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<StockTakeDetail> StockTakeDetails { get; set; } = new List<StockTakeDetail>();

    public virtual Warehouse Warehouse { get; set; } = null!;
}
