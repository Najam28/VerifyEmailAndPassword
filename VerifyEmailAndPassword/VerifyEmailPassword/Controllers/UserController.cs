using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace VerifyEmailPassword.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;

        public UserController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequest registerRequest) 
        {
            if(_context.Users.Any(u => u.Email == registerRequest.Email)) 
            {
                return BadRequest("User already exists");
            }
            CreatePasswordHash(registerRequest.Password!, out byte[] passwordHash, out byte[] passwordSalt);
            var user = new User 
            { 
                Email = registerRequest.Email,
                Password = registerRequest.Password,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt, 
                VerificationToken = CreateRandomToken()
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("User Successfully Created");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserRegisterRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if(user == null)
            {
                return BadRequest("Not Found");
            }
            if(!VerifyPasswordHash(request.Password!, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Password is Incorrect");
            }
            if(user.VerifyAt == null)
            {
                return BadRequest("Not Verified");
            }
            return Ok("Welcome Back " + user.Email!);
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if(user == null)
            {
                return BadRequest("Invalid Token");
            }
            user.VerifyAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok("User Verified");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if(user == null)
            {
                return BadRequest("User not found.");
            }
            user.ResetToken = CreateRandomToken();
            user.ResetTokenExpire = DateTime.Now.AddDays(1);
            await _context.SaveChangesAsync();
            return Ok("You may now reset password");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(UserRegisterRequest registerRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == registerRequest.Token);
            if(user == null || user.ResetTokenExpire < DateTime.Now)
            {
                return BadRequest("Invalid Token.");
            }
            CreatePasswordHash(registerRequest.Password!, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.Password = registerRequest.Password;
            user.ResetToken = null;
            user.ResetTokenExpire = null;
            await _context.SaveChangesAsync();
            return Ok("Password Reset Successfully");
        }
        private void CreatePasswordHash(string password, out byte[] passwordSalt, out byte[] passwordHash) 
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPasswordHash(string password, byte[] passwordSalt, byte[] passwordHash) 
        {
            using(var hmac = new HMACSHA512(passwordSalt))
            {
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(passwordHash);
            }
        }
        private string CreateRandomToken() 
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }
    }
}
