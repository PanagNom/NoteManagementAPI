namespace NoteManagementAPI.DTOs
{
    /// <summary>
    /// Contains a short-lived access token and a one-time refresh token.
    /// </summary>
    public sealed class TokenResponseDTO
    {
        /// <summary>Gets the JWT used to authorize API requests.</summary>
        public string AccessToken { get; init; } = string.Empty;

        /// <summary>Gets the UTC expiration of the access token.</summary>
        public DateTimeOffset AccessTokenExpiresAtUtc { get; init; }

        /// <summary>Gets the opaque one-time token used to renew the session.</summary>
        public string RefreshToken { get; init; } = string.Empty;

        /// <summary>Gets the fixed UTC expiration of the refresh-token family.</summary>
        public DateTimeOffset RefreshTokenExpiresAtUtc { get; init; }
    }
}
