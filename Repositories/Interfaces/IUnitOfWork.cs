namespace NoteManagementAPI.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        INoteRepository _noteRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
