using System.ComponentModel.DataAnnotations;

namespace VerifyEmailPassword.Models
{
    public class UserRegisterRequest
    {
        [Required, EmailAddress]
        public string? Email { get; set; }
        public string? Token { get; set; }
        [Required, MinLength(6, ErrorMessage = "Please enter more then 6 letter")]
        public string? Password { get; set; }
        [Required, Compare("Password")]
        public string? ConfirmPassword { get; set; }
    }
}
