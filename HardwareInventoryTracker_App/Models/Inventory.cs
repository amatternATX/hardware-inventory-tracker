using System;
using System.Collections.Generic;

namespace HardwareInventoryTracker_App.Models;

public partial class Inventory
{
    public int Id { get; set; }

    public int Quantity { get; set; }

    public int HardwareItemId { get; set; }

    public virtual HardwareItem HardwareItem { get; set; } = null!;
}
