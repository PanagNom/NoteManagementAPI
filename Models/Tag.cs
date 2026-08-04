namespace NoteManagementAPI.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public string CreatedBy { get; set; } = default!;
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; } = default!;
    }
}
