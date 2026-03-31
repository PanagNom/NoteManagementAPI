using Microsoft.EntityFrameworkCore;
using NoteManagementAPI.DTOs;
using NoteManagementAPI.Infrastructure;
using NoteManagementAPI.Models;
using NoteManagementAPI.Repositories.Interfaces;

namespace NoteManagementAPI.Repositories
{
    public class NoteRepository: INoteRepository
    {
        private readonly NoteDbContext _context;

        public NoteRepository(NoteDbContext context)
        {
            _context = context;
        }

        public async Task<Note?> Get(int Id)
        {
            return await _context.Notes.FirstOrDefaultAsync(note => note.Id==Id);
        }

        public async Task<IEnumerable<Note>?> GetAll()
        {
            return _context.Notes.ToList();
        }

        public async Task Create(Note noteToCreate)
        {
            await _context.Notes.AddAsync(noteToCreate);
        }

        public void Update(Note noteToUpdate)
        {
            _context.Notes.Update(noteToUpdate);
        }

        public async Task Delete(int Id)
        {
            Note? noteToDelete = await _context.Notes.FindAsync(Id);

            if (noteToDelete == null)
            {
                throw new Exception("Invalid Id");
            }

            _context.Notes.Remove(noteToDelete);
        }

        public Task<bool> Exists(int Id)
        {
            return(_context.Notes.AnyAsync(note => note.Id == Id));
        }
    }
}
