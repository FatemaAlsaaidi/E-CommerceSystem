using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public class RefreshTokenRepo : IRefreshTokenRepo
    {
        private readonly ApplicationDbContext _ctx;
        public RefreshTokenRepo(ApplicationDbContext ctx) => _ctx = ctx;

        public void Add(RefreshToken token) { _ctx.RefreshTokens.Add(token); }
        public RefreshToken? GetByHash(string tokenHash)
            => _ctx.RefreshTokens.FirstOrDefault(x => x.TokenHash == tokenHash);

        public IEnumerable<RefreshToken> GetActiveByUser(int uid)
            => _ctx.RefreshTokens.Where(x => x.UID == uid && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow).ToList();

        public void Update(RefreshToken token) { _ctx.RefreshTokens.Update(token); }
        public void Revoke(RefreshToken token) { token.RevokedAtUtc = DateTime.UtcNow; _ctx.RefreshTokens.Update(token); }
        public void Save() => _ctx.SaveChanges();
    }
}
