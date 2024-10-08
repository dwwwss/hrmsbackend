using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Nationality
{
    public int NationalityId { get; set; }

    public string NationalityName { get; set; } = null!;
}
