using System;
using System.Collections.Generic;

namespace Pr3_Gordov.API.Models;

public partial class ClothingType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Clothing> Clothings { get; set; } = new List<Clothing>();
}
