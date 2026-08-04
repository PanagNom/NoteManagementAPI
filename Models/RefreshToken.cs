using System.ComponentModel.DataAnnotations;

namespace NoteManagementAPI.Models
{
    internal sealed class RefreshToken
    {
        private RefreshToken()
        {
        }

        public RefreshToken(
            string tokenHash,
            string userId,
            Guid familyId,
            DateTimeOffset createdAtUtc,
            DateTimeOffset expiresAtUtc)
        {
            if (string.IsNullOrWhiteSpace(tokenHash))
            {
                throw new ArgumentException("Token hash is required.", nameof(tokenHash));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User id is required.", nameof(userId));
            }

            if (expiresAtUtc <= createdAtUtc)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(expiresAtUtc),
                    "Refresh token expiration must be after its creation time.");
            }

            TokenHash = tokenHash;
            UserId = userId;
            FamilyId = familyId;
            CreatedAtUtc = createdAtUtc;
            ExpiresAtUtc = expiresAtUtc;
        }

        public long Id { get; private set; }

        [MaxLength(64)]
        public string TokenHash { get; private set; } = string.Empty;

        public string UserId { get; private set; } = string.Empty;
        public ApplicationUser User { get; private set; } = default!;
        public Guid FamilyId { get; private set; }
        public DateTimeOffset CreatedAtUtc { get; private set; }
        public DateTimeOffset ExpiresAtUtc { get; private set; }
        public DateTimeOffset? RevokedAtUtc { get; private set; }

        [MaxLength(64)]
        public string? ReplacedByTokenHash { get; private set; }

        [Timestamp]
        public byte[] RowVersion { get; private set; } = [];

        public bool IsActive(DateTimeOffset now)
        {
            return RevokedAtUtc == null && ExpiresAtUtc > now;
        }

        public void Revoke(DateTimeOffset revokedAtUtc, string? replacedByTokenHash = null)
        {
            if (RevokedAtUtc != null)
            {
                return;
            }

            RevokedAtUtc = revokedAtUtc;
            ReplacedByTokenHash = replacedByTokenHash;
        }
    }
}
