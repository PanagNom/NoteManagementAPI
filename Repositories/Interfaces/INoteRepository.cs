using NoteManagementAPI.Models;
using NoteManagementAPI.Services;

namespace NoteManagementAPI.Repositories.Interfaces
{
    public interface INoteRepository
    {
        Task<Note?> GetNoteAsync(int noteId, string ownerUserId, bool includeTags = false);
        Task<(IEnumerable<Note> Notes, PaginationMetadata PaginationMetadata)> GetNotesAsync(
            string ownerUserId,
            string? title,
            string? searchQuery,
            int pageNumber,
            int pageSize);
        Task Create(Note noteToCreate);
        void Update(Note noteToUpdate);
        void Delete(Note noteToDelete);
    }
}
