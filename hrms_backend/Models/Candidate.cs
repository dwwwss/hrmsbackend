using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Candidate
{
    public int CandidateId { get; set; }

    public string? Fullname { get; set; }

    public string? Email { get; set; }

    public string? MobileNo { get; set; }

    public byte[]? Cv { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? JoinDate { get; set; }

    public string? StagesName { get; set; }

    public int? FkCompanyId { get; set; }

    public int? FkJobId { get; set; }

    public string? Source { get; set; }

    public string? Resume { get; set; }

    public string? Skills { get; set; }

    public string? ProfilePhoto { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Company? FkCompany { get; set; }

    public virtual Job? FkJob { get; set; }
}
