using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;

namespace NoteManagementAPI.Repositories.Interfaces
{
    public interface INoteRepository
    {
        Task<NoteDTO> Get(int Id);
        Task<IEnumerable<NoteDTO>?> GetAll();
        Task Create(Note noteToCreate);
        Task Update(NoteDTO noteToUpdate);
        Task Delete(int Id);
    }
}
