using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IAuthService
    {
        string CreateAccessToken(User user);      // short-lived JWT
        (string raw, string hash, DateTime expiresUtc) CreateRefreshToken(); // long-lived random secret
        void RotateRefreshToken(int uid, string oldTokenRaw, out string newRaw, out DateTime newExpUtc);
        void RevokeRefreshToken(string tokenRaw);
        string Sha256(string value);
    }
}
