namespace NoteManagementAPI.DTOs
{
    public class NoteWithoutTagsDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
    }
}
