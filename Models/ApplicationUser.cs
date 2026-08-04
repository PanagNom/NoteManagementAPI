using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NoteManagementAPI.Models
{
    public sealed class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
