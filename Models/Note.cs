namespace NoteManagementAPI.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public IEnumerable<Tag>? Tags { get; set; } = default!;
        public string CreatedBy { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string ModifieddBy { get; set; } = default!;
        public DateTime ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
