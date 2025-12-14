using System.ComponentModel.DataAnnotations.Schema;

namespace TaskPanel.ViewModels
{
    [NotMapped]
    public class TaskWithUserVM
    {
        public int NTaskNo { get; set; }
        public string? CTask { get; set; }
        public DateTime? DTaskDate { get; set; }
        public DateTime? DDeadLine { get; set; }
        public string? FromUserName { get; set; }
        public string? ToUserName { get; set; }

        // New props for due status
        public string? DueStatusColor { get; set; }
        public string? DueStatusText { get; set; }
        public string? CFileName { get; set; }

    }
}
