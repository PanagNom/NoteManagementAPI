using System.ComponentModel.DataAnnotations;

namespace NoteManagementAPI.DTOs
{
    public sealed class LoginRequestDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
