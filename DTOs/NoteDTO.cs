using NoteManagementAPI.Models;

namespace NoteManagementAPI.DTOs
{
    public class NoteDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public IEnumerable<TagInNoteDTO>? Tags { get; set; }
    }
}
