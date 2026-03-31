using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;

namespace NoteManagementAPI.Repositories.Interfaces
{
    public interface INoteRepository
    {
        Task<Note?> Get(int Id);
        Task<IEnumerable<Note>?> GetAll();
        Task<bool> Exists(int Id);
        Task Create(Note noteToCreate);
        void Update(Note noteToUpdate);
        Task Delete(int Id);
    }
}
