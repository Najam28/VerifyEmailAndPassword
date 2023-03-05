namespace VerifyEmailPassword.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public byte[] PasswordHash { get; set; } = new byte[32];
        public byte[] PasswordSalt { get; set; } = new byte[32];
        public string? VerificationToken { get; set; }
        public DateTime? VerifyAt { get; set; }
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpire { get; set; }
    }
}
