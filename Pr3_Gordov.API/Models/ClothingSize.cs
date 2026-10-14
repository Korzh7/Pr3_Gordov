using System;
using System.Collections.Generic;

namespace Pr3_Gordov.API.Models;

public partial class ClothingSize
{
    public long Id { get; set; }

    public long ClothingId { get; set; }

    public string LetterSize { get; set; } = null!;

    public int NumericSize { get; set; }

    public int StockQuantity { get; set; }

    public virtual Clothing Clothing { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
