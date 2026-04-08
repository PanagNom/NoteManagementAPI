using NoteManagementAPI.Models;

namespace NoteManagementAPI.DTOs
{
    public class TagDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public IEnumerable<Note> Notes { get; set; }
    }
}
