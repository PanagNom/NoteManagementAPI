using NoteManagementAPI.Models;

namespace NoteManagementAPI.Repositories.Interfaces
{
    public interface ITagRepository
    {
        Task<Tag?> Get(int Id);
        Task<IEnumerable<Tag>?> GetAll();
        Task<bool> Exists(int Id);
        Task Create(Tag tagToCreate);
        void Update(Tag tagToUpdate);
        Task Delete(int Id);
    }
}
