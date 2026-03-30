namespace NoteManagementAPI.DTOs
{
    public class NoteDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public int CharacterCount { get; set; }
        public string[]? Tags { get; set; }
    }
}
