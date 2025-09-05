using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace E_CommerceSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _cfg;
        private readonly IRefreshTokenRepo _refresh;
        private readonly IUserRepo _users;

        public AuthService(IConfiguration cfg, IRefreshTokenRepo refresh, IUserRepo users)
            => (_cfg, _refresh, _users) = (cfg, refresh, users);

        public string CreateAccessToken(User user)
        {
            var jwtSection = _cfg.GetSection("JwtSettings");
            var secretKey = jwtSection["SecretKey"]!;
            var expiryMin = double.Parse(jwtSection["ExpiryInMinutes"]!);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UID.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMin),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (string raw, string hash, DateTime expiresUtc) CreateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            var raw = Convert.ToBase64String(bytes);
            return (raw, Sha256(raw), DateTime.UtcNow.AddDays(14)); // 14-day refresh
        }

        public void RotateRefreshToken(int uid, string oldTokenRaw, out string newRaw, out DateTime newExpUtc)
        {
            var oldHash = Sha256(oldTokenRaw);
            var rt = _refresh.GetByHash(oldHash) ?? throw new UnauthorizedAccessException("Invalid refresh token.");
            if (!rt.IsActive || rt.UID != uid) throw new UnauthorizedAccessException("Refresh token inactive.");

            // issue new
            var (raw, hash, exp) = CreateRefreshToken();
            newRaw = raw; newExpUtc = exp;

            var replacement = new RefreshToken
            {
                UID = uid,
                TokenHash = hash,
                ExpiresAtUtc = exp,
                CreatedAtUtc = DateTime.UtcNow
            };
            _refresh.Add(replacement);

            // revoke old & record chain
            rt.RevokedAtUtc = DateTime.UtcNow;
            rt.ReplacedByHash = hash;
            _refresh.Update(rt);

            _refresh.Save();
        }

        public void RevokeRefreshToken(string tokenRaw)
        {
            var hash = Sha256(tokenRaw);
            var rt = _refresh.GetByHash(hash);
            if (rt is null) return;
            _refresh.Revoke(rt);
            _refresh.Save();
        }

        public string Sha256(string value)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToBase64String(bytes);
        }
    }
}
