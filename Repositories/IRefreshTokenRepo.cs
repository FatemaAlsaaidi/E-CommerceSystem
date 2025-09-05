using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface IRefreshTokenRepo
    {
        void Add(RefreshToken token);
        RefreshToken? GetByHash(string tokenHash);
        IEnumerable<RefreshToken> GetActiveByUser(int uid);
        void Update(RefreshToken token);
        void Revoke(RefreshToken token);
        void Save();
    }
}
