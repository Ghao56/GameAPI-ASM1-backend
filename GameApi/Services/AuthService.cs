using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameApi.Data;
using GameApi.DTOs;
using GameApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GameApi.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<Account> _passwordHasher = new();

    public AuthService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<AccountResponse?> RegisterAsync(RegisterRequest request)
    {
        if (await _db.Accounts.AnyAsync(a => a.Username == request.Username))
        {
            return null;
        }

        var account = new Account
        {
            Username = request.Username,
            CreatedAt = DateTime.UtcNow
        };
        account.PasswordHash = _passwordHasher.HashPassword(account, request.Password);

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        return new AccountResponse { Id = account.Id, Username = account.Username };
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Username == request.Username);
        if (account is null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return new LoginResponse { Token = CreateToken(account), Username = account.Username };
    }

    private string CreateToken(Account account)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is missing.");

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Name, account.Username)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
