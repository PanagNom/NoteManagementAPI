using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;

namespace NoteManagementAPI.Repositories.Interfaces
{
    public interface INoteRepository
    {
        Task<Note?> GetNoteAsync(int noteId, bool includeTags);
        Task<IEnumerable<Note>?> GetNotesAsync();
        Task<bool> NoteExistsAsync(int noteId);
        Task Create(Note noteToCreate);
        Task Update(Note noteToUpdate);
        Task DeleteNote(int noteId);
    }
}
