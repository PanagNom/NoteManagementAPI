using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;

namespace NoteManagementAPI.Services
{
    /// <summary>
    /// Issues, rotates, and revokes authentication token pairs.
    /// </summary>
    public interface IAuthenticationTokenService
    {
        /// <summary>Issues a new access/refresh pair for a signed-in user.</summary>
        Task<TokenResponseDTO> IssueTokenPairAsync(
            ApplicationUser user,
            CancellationToken cancellationToken);

        /// <summary>Rotates a valid refresh token, or returns null when it cannot be used.</summary>
        Task<TokenResponseDTO?> RotateRefreshTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken);

        /// <summary>Revokes a refresh-token family when it belongs to the supplied user.</summary>
        Task RevokeRefreshTokenFamilyAsync(
            string refreshToken,
            string userId,
            CancellationToken cancellationToken);
    }
}
