using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Barber> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<Barber> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByNameAsync(dto.UserName);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Username already exists" });
        }

        var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingEmail != null)
        {
            return BadRequest(new { message = "Email already exists" });
        }

        var user = new Barber
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            firstName = dto.FirstName,
            lastName = dto.LastName,
        };
        if (!string.IsNullOrEmpty(dto.BirthDate))
        {
            if (DateOnly.TryParse(dto.BirthDate, out var parsedDate))
            {
                user.BirthDate = parsedDate;
            }
            else
            {
                return BadRequest(new { message = "Invalid birth date format. Use YYYY-MM-DD." });
            }
        }

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        await _userManager.AddToRoleAsync(user, "Barber");

        return Ok(new { message = "Barber registered successfully" });
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid username" });
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
        {
            return Unauthorized(new { message = "Invalid password" });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        Response.Cookies.Append(
            "jwt",
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["Jwt:ExpiresInMinutes"]!)
                ),
            }
        );

        return Ok(
            new
            {
                roles,
                userId = user.Id,
                username = user.UserName,
            }
        );
    }

    [HttpPost("Logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt");
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("Me")]
    [Authorize]
    public async Task<ActionResult<Barber>> GetCurrentUser()
    {
        var userId = User.FindFirst("id")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    [HttpGet("GetUserData/{id}")]
    public async Task<ActionResult<Barber>> GetUserData(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound($"User don't  exists!");
        }

        return Ok(user);
    }

    [HttpPut("UpdateUser")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updatedUser)
    {
        var user = await _userManager.FindByIdAsync(updatedUser.Id);
        if (user == null)
            return NotFound(new { message = $"User with Id {updatedUser.Id} not found" });

        if (!string.Equals(user.UserName, updatedUser.UserName, StringComparison.OrdinalIgnoreCase))
        {
            var userWithSameName = await _userManager.FindByNameAsync(updatedUser.UserName!);
            if (userWithSameName != null && userWithSameName.Id != user.Id)
            {
                return BadRequest(new { message = "Username already exists" });
            }
        }

        if (!string.Equals(user.Email, updatedUser.Email, StringComparison.OrdinalIgnoreCase))
        {
            var userWithSameEmail = await _userManager.FindByEmailAsync(updatedUser.Email!);
            if (userWithSameEmail != null && userWithSameEmail.Id != user.Id)
            {
                return BadRequest(new { message = "Email already exists" });
            }
        }

        user.firstName = updatedUser.FirstName;
        user.lastName = updatedUser.LastName;
        user.Email = updatedUser.Email;
        user.UserName = updatedUser.UserName;
        user.PhoneNumber = updatedUser.PhoneNumber;

        if (!string.IsNullOrEmpty(updatedUser.BirthDate))
        {
            if (DateOnly.TryParse(updatedUser.BirthDate, out var parsedDate))
            {
                user.BirthDate = parsedDate;
            }
            else
            {
                return BadRequest(new { message = "Invalid birth date format. Use YYYY-MM-DD." });
            }
        }

        if (user.UserName != null)
            user.NormalizedUserName = _userManager.NormalizeName(user.UserName);
        if (user.Email != null)
            user.NormalizedEmail = _userManager.NormalizeEmail(user.Email);

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(user);
    }

    private string GenerateJwtToken(Barber user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim("id", user.Id),
            new Claim("username", user.UserName!),
            new Claim("email", user.Email!),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiresInMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("CheckPassword")]
    public async Task<IActionResult> CheckPassword([FromBody] CheckPasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
        {
            return NotFound(new { message = $"User with Id {dto.UserId} not found" });
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
        {
            return BadRequest(new { message = "Incorrect password" });
        }

        return Ok(new { message = "Password is correct" });
    }

    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
        {
            return NotFound(new { message = $"User with Id {dto.UserId} not found" });
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            dto.CurrentPassword,
            dto.NewPassword
        );

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { message = "Password changed successfully" });
    }

    [HttpGet("GetByUsername/{username}")]
    public async Task<ActionResult<Barber>> GetByUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest(new { message = "Username cannot be empty." });
        }

        var user = await _userManager.FindByNameAsync(username);

        if (user == null)
        {
            return NotFound(new { message = $"Barber with username @{username} does not exist." });
        }

        return Ok(user);
    }
}
