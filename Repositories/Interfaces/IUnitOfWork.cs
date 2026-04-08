namespace NoteManagementAPI.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        INoteRepository NoteRepository { get; }
        ITagRepository TagRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
