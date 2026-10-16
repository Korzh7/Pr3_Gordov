using System;
using System.Collections.Generic;

namespace Pr3_Gordov.API.Models;

public partial class Order
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User User { get; set; } = null!;
}
