namespace NoteManagementAPI.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        INoteRepository NoteRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
