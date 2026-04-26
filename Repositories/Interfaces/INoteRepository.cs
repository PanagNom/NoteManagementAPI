using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;
using NoteManagementAPI.Services;

namespace NoteManagementAPI.Repositories.Interfaces
{
    public interface INoteRepository
    {
        Task<Note?> GetNoteAsync(int noteId, bool includeTags = false);
        Task<IEnumerable<Note>?> GetNotesAsync();
        Task<(IEnumerable<Note>?, PaginationMetadata)> GetNotesAsync(string? title, string? searchQuery, int pageNumber, int pageSize);
        Task<bool> NoteExistsAsync(int noteId);
        Task Create(Note noteToCreate);
        Task Update(Note noteToUpdate);
        Task DeleteNote(int noteId);
    }
}
