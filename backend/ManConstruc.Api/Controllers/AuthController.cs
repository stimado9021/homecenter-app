using System.Security.Claims;
using ManConstruc.Api.Contracts;
using ManConstruc.Api.Data;
using ManConstruc.Api.Models;
using ManConstruc.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManConstruc.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;

    public AuthController(AppDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(user => user.Email == email))
        {
            return BadRequest(new { message = "Ya existe una cuenta con ese correo." });
        }

        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            CompanyName = (request.CompanyName ?? string.Empty).Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(_tokens.CreateAuthResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(candidate => candidate.Email == email);

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Correo o contraseña incorrectos." });
        }

        return Ok(_tokens.CreateAuthResponse(user));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var userId = User.FindFirstValue("sub");
        if (!Guid.TryParse(userId, out var id))
        {
            return Unauthorized(new { message = "Sesión no válida." });
        }

        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == id);
        if (user is null)
        {
            return Unauthorized(new { message = "La cuenta ya no existe." });
        }

        return Ok(new UserDto(user.Id, user.Email, user.FullName, user.CompanyName));
    }
}
