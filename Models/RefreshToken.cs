using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_CommerceSystem.Models
{
    public class RefreshToken
    {
        [Key] public int Id { get; set; }

        [Required] public int UID { get; set; }          // FK to User
        [ForeignKey(nameof(UID))] public virtual User User { get; set; } = default!;

        // Store only a hash of the token for safety
        [Required] public string TokenHash { get; set; } = default!;

        [Required] public DateTime ExpiresAtUtc { get; set; }
        [Required] public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAtUtc { get; set; }
        public string? ReplacedByHash { get; set; }      // for rotation
        public bool IsActive => RevokedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc;
    }
}
