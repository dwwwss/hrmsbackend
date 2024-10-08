using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class City
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int StateId { get; set; }

    public virtual Statess State { get; set; } = null!;
}
