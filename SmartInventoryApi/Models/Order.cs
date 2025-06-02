using System;
using System.Collections.Generic;

namespace SmartInventoryApi.Models;

public partial class Order
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string OrderType { get; set; } = null!;

    public int PartnerId { get; set; }

    public int WarehouseId { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Partner Partner { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
