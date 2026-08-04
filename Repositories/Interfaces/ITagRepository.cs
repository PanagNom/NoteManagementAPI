using NoteManagementAPI.Models;

namespace NoteManagementAPI.Repositories.Interfaces
{
    public interface ITagRepository
    {
        Task<Tag?> GetTagAsync(int tagId, string ownerUserId, bool includeNotes = false);
        Task<IEnumerable<Tag>> GetTagsAsync(string ownerUserId, bool includeNotes = false);
        Task<bool> TagNameExistsAsync(string ownerUserId, string name, int? excludingTagId = null);
        Task CreateTagAsync(Tag tagToCreate);
        void UpdateTag(Tag tagToUpdate);
        void DeleteTag(Tag tagToDelete);
    }
}
