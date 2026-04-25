namespace NoteManagementAPI.DTOs
{
    public class NoteCreationDTO
    {
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public IEnumerable<TagInNoteDTO>? Tags { get; set; }
    }
}
