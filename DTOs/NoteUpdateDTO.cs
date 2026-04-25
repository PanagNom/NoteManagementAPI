namespace NoteManagementAPI.DTOs
{
    public class NoteUpdateDTO
    {
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public IEnumerable<TagInNoteDTO>? Tags { get; set; }
    }
}
