using System;
using System.Collections.Generic;

namespace TaskPanel.Models;

public partial class GenDailyTask
{
    public int NUcode { get; set; }

    public int? NTask { get; set; }

    public string? CDailyTask { get; set; }
    public int? NUserId{ get; set; }
}
