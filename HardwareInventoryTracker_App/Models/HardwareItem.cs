using System;
using System.Collections.Generic;

namespace HardwareInventoryTracker_App.Models;

public partial class HardwareItem
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public int PackageQuantitiy { get; set; }

    public int SuggestedOrder { get; set; }

    public int? ReorderThreshold { get; set; }

    public string? ReorderLink { get; set; }

    public string? ProductImageUrl { get; set; }

    public int Quantity { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<OrderHistory> OrderHistories { get; set; } = new List<OrderHistory>();
}
