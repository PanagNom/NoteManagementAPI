using System.ComponentModel.DataAnnotations;

namespace NoteManagementAPI.DTOs
{
    public class TagUpdateDTO
    {
        [Required(ErrorMessage = "You should provide a name value.")]
        [MaxLength(50)]
        public string Name { get; set; } = default!;
    }
}
