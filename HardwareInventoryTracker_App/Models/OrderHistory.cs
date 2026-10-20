using System;
using System.Collections.Generic;

namespace HardwareInventoryTracker_App.Models;

public partial class OrderHistory
{
    public int Id { get; set; }

    public DateOnly OrderDate { get; set; }

    public int? FulfillmentDate { get; set; }

    public int OrderQuantity { get; set; }

    public int HardwareItemId { get; set; }

    public decimal? OrderPrice { get; set; }

    public decimal? OrderTotal { get; set; }

    public virtual HardwareItem HardwareItem { get; set; } = null!;
}
