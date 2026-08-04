using System.ComponentModel.DataAnnotations;

namespace NoteManagementAPI.DTOs
{
    /// <summary>
    /// Supplies a refresh token for rotation or session revocation.
    /// </summary>
    public sealed class RefreshTokenRequestDTO
    {
        /// <summary>
        /// Gets or sets the opaque refresh token returned by authentication.
        /// </summary>
        [Required]
        [MinLength(32)]
        [MaxLength(512)]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
