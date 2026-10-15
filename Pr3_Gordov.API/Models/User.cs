using System;
using System.Collections.Generic;

namespace Pr3_Gordov.API.Models;

public partial class User
{
    public long Id { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string DeliveryAddress { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
