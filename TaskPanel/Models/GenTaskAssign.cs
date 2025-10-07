using System;
using System.Collections.Generic;

namespace TaskPanel.Models;

public partial class GenTaskAssign
{
    public int NTaskNo { get; set; }

    public string? CTask { get; set; }

    public DateTime? DTaskDate { get; set; }

    public bool? NComplete { get; set; }

    public DateTime? DCompleteDate { get; set; }

    public bool? NApprove { get; set; }

    public DateTime? DApprove { get; set; }

    public string? CFileName { get; set; }

    public DateTime? DDeadLine { get; set; }

    public int? NFromUser { get; set; }

    public int? NToUser { get; set; }
}

public class TaskWithUserVM
{
    public int NTaskNo { get; set; }
    public string? CTask { get; set; }
    public DateTime? DTaskDate { get; set; }
    public DateTime? DDeadLine { get; set; }
    public string? FromUserName { get; set; }
    public string? ToUserName { get; set; }

    // for due status
    public string? DueStatusColor { get; set; }
    public string? DueStatusText { get; set; }
}