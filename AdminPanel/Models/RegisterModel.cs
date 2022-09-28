using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "The username cannot be blank.")]
        public string Username { get; set; } = default!;

        [Required(ErrorMessage = "The E-mail cannot be blank.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Please enter a valid E-mail address.")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "The password cannot be blank.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;
    }
}
