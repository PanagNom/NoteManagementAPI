using Microsoft.EntityFrameworkCore;
using NoteManagementAPI.DTOs;
using NoteManagementAPI.Infrastructure;
using NoteManagementAPI.Models;
using NoteManagementAPI.Repositories.Interfaces;

namespace NoteManagementAPI.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly NoteDbContext _context;

        public TagRepository(NoteDbContext context)
        {
            _context = context;
        }

        public async Task Create(Tag tagToCreate)
        {
            await _context.Tags.AddAsync(tagToCreate);
        }

        public async Task Delete(int Id)
        {
            Tag? tagToDelete = await _context.Tags.FindAsync(Id);
            if (tagToDelete == null) 
            {
                throw new Exception("Invalid Id");
            }
            _context.Tags.Remove(tagToDelete);
        }

        public Task<bool> Exists(int Id)
        {
            return (_context.Tags.AnyAsync(tag=>tag.Id ==Id));
        }

        public async Task<Tag?> Get(int Id)
        {
            return await _context.Tags.FirstOrDefaultAsync(tag => tag.Id == Id);
        }

        public async Task<IEnumerable<Tag>?> GetAll()
        {
            return _context.Tags.ToList();
        }

        public void Update(Tag tagToUpdate)
        {
            _context.Tags.Update(tagToUpdate);
        }
    }
}
