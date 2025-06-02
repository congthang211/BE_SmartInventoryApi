using System;
using System.Collections.Generic;

namespace SmartInventoryApi.Models;

public partial class StockTakeDetail
{
    public int Id { get; set; }

    public int StockTakeId { get; set; }

    public int ProductId { get; set; }

    public int SystemQuantity { get; set; }

    public int? CountedQuantity { get; set; }

    public int? Variance { get; set; }

    public string? Notes { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual StockTake StockTake { get; set; } = null!;
}
