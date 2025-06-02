using System;
using System.Collections.Generic;

namespace SmartInventoryApi.Models;

public partial class ActivityLog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Action { get; set; } = null!;

    public string Module { get; set; } = null!;

    public string? Description { get; set; }

    public int? EntityId { get; set; }

    public string? EntityType { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime LogDate { get; set; }

    public virtual User User { get; set; } = null!;
}
