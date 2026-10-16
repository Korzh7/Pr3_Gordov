using System;
using System.Collections.Generic;

namespace Pr3_Gordov.API.Models;

public partial class Clothing
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public int PurchasesCount { get; set; }

    public int ClothingTypeId { get; set; }

    public bool IsAvailable { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ClothingSize> ClothingSizes { get; set; } = new List<ClothingSize>();

    public virtual ClothingType ClothingType { get; set; } = null!;
}
