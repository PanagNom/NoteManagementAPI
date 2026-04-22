using NoteManagementAPI.Models;

namespace NoteManagementAPI.Repositories.Interfaces
{
    public interface ITagRepository
    {
        Task<Tag?> GetTagAsync(int Id, bool includeNotes = false);
        Task<IEnumerable<Tag>?> GetTagsAsync(bool includeNotes = false);
        Task<bool> TagExistsAsync(int Id);
        Task CreateTagAsync(Tag tagToCreate);
        void UpdateTag(Tag tagToUpdate);
        Task DeleteTagAsync(int Id);
    }
}
