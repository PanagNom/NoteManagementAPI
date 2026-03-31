using NoteManagementAPI.Infrastructure;
using NoteManagementAPI.Repositories.Interfaces;

namespace NoteManagementAPI.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public INoteRepository _noteRepository { get; }
        private readonly NoteDbContext _context;

        public UnitOfWork(NoteDbContext context, INoteRepository noteRepository)
        {
            _context = context;
            _noteRepository = noteRepository;
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
