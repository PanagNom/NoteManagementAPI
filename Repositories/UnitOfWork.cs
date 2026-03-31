using NoteManagementAPI.Infrastructure;
using NoteManagementAPI.Repositories.Interfaces;

namespace NoteManagementAPI.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public INoteRepository NoteRepository { get; }
        private readonly NoteDbContext _context;

        public UnitOfWork(NoteDbContext context, INoteRepository noteRepository)
        {
            _context = context;
            NoteRepository = noteRepository;
        }
        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
