using System;
using System.Collections.Generic;

namespace HardwareInventoryTracker_App.Models;

public partial class Category
{
    public int Id { get; set; }

    public string? CategoryName { get; set; }

    public string? CategoryType { get; set; }

    public virtual ICollection<HardwareItem> HardwareItems { get; set; } = new List<HardwareItem>();
}
