using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class State
{
    public int StateId { get; set; }

    public string StateName { get; set; } = null!;

    public int? CountryId { get; set; }

    public virtual Country1? Country { get; set; }
}
