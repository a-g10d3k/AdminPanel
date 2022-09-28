using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "The username cannot be blank.")]
        public string Username { get; set; } = default!;

        [Required(ErrorMessage = "The password cannot be blank.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;
    }
}
