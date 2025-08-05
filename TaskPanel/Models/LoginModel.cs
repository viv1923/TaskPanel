using System.ComponentModel.DataAnnotations;

namespace TaskPanel.Models
{
    public class LoginModel
    {
        [Required]
        public string CUserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string CPassword { get; set; }

    }
}
