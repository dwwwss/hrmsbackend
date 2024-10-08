using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Country1
{
    public int CountryId { get; set; }

    public string CountryName { get; set; } = null!;

    public virtual ICollection<State> States { get; set; } = new List<State>();
}
