namespace NoteManagementAPI.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public string OwnerUserId { get; private set; } = default!;
        public ApplicationUser Owner { get; private set; } = default!;
        public string CreatedBy { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; } = default!;
        public DateTime ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }

        public void AssignOwner(string ownerUserId)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                throw new ArgumentException("Owner user id is required.", nameof(ownerUserId));
            }

            if (!string.IsNullOrEmpty(OwnerUserId))
            {
                throw new InvalidOperationException("The note owner cannot be changed.");
            }

            OwnerUserId = ownerUserId;
        }
    }
}
