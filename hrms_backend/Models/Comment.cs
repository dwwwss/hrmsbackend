using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public string? Text { get; set; }

    public int? FkCandidateId { get; set; }

    public int? FkCompanyId { get; set; }

    public int? FkEmployeeId { get; set; }

    public TimeSpan? Time { get; set; }

    public virtual Candidate? FkCandidate { get; set; }

    public virtual Company? FkCompany { get; set; }

    public virtual Employee? FkEmployee { get; set; }
}
