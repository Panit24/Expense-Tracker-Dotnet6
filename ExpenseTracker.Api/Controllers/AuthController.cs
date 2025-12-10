using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Services;
using BCrypt.Net;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AppDbContext db, 
            IJwtService jwtService,
            ILogger<AuthController> logger)
        {
            _db = db;
            _jwtService = jwtService;
            _logger = logger;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                // Check if user already exists
                if (await _db.Users.AnyAsync(u => u.Username == registerDto.Username))
                {
                    return BadRequest(new { message = "Username already taken" });
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
                Console.WriteLine("registerDto" +  registerDto);
                // Create user
                var user = new User
                {
                    Username = registerDto.Username,
                    Password = passwordHash,
                    PhoneNumber = registerDto.PhoneNumber,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);
                var expiresAt = _jwtService.GetTokenExpiration();

                var response = new AuthResponseDto
                {
                    UserId = user.Id,
                    Username = user.Username,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    Token = token,
                    ExpiresAt = expiresAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // Find user by username
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);

                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);
                var expiresAt = _jwtService.GetTokenExpiration();

                var response = new AuthResponseDto
                {
                    UserId = user.Id,
                    Username = user.Username,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    Token = token,
                    ExpiresAt = expiresAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        // GET: api/auth/me
        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized();
                }

                var user = await _db.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = user.CreatedAt
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}