using System;
using System.Collections.Generic;

namespace Pr3_Gordov.API.Models;

public partial class OrderItem
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public long ClothingSizeId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public virtual ClothingSize ClothingSize { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
