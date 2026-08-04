using System.ComponentModel.DataAnnotations;

namespace NoteManagementAPI.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; } = default!;
        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public string OwnerUserId { get; private set; } = default!;
        public ApplicationUser Owner { get; private set; } = default!;
        public string CreatedBy { get; set; } = default!;
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; } = default!;

        public void AssignOwner(string ownerUserId)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                throw new ArgumentException("Owner user id is required.", nameof(ownerUserId));
            }

            if (!string.IsNullOrEmpty(OwnerUserId))
            {
                throw new InvalidOperationException("The tag owner cannot be changed.");
            }

            OwnerUserId = ownerUserId;
        }
    }
}
