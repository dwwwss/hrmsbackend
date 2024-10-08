using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Country
{
    public int Id { get; set; }

    public string Shortname { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int Phonecode { get; set; }

    public virtual ICollection<Statess> Statesses { get; set; } = new List<Statess>();
}
