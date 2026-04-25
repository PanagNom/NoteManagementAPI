using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;

namespace NoteManagementAPI.Repositories.Interfaces
{
    public interface INoteRepository
    {
        Task<Note?> GetNoteAsync(int noteId, bool includeTags = false);
        Task<IEnumerable<Note>?> GetNotesAsync();
        Task<IEnumerable<Note>?> GetNotesAsync(string? title);
        Task<bool> NoteExistsAsync(int noteId);
        Task Create(Note noteToCreate);
        Task Update(Note noteToUpdate);
        Task DeleteNote(int noteId);
    }
}
