namespace NoteManagementAPI.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public int CharacterCount { get; set; }
        public string[]? Tags { get; set; }
        public string CreatedBy { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string ModifieddBy { get; set; } = default!;
        public DateTime ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
