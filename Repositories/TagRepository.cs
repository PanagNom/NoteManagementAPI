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

        public async Task<IEnumerable<Tag>?> GetTagsAsync(bool includeNotes = false)
        {
            var query = _context.Tags.OrderBy(tag => tag.Name).AsQueryable();

            if (includeNotes)
            {
                query = query.Include(t => t.Notes);
            }

            return await query.ToListAsync();
        }

        public async Task<Tag?> GetTagAsync(int tagId, bool includeNotes= false)
        {
            var query = _context.Tags.Where(tag => tag.Id == tagId).AsQueryable();

            if (includeNotes)
            {
                query = query.Include(t => t.Notes);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task CreateTagAsync(Tag tagToCreate)
        {
            await _context.Tags.AddAsync(tagToCreate);
        }

        public async Task DeleteTagAsync(int tagId)
        {
            Tag? tagToDelete = await _context.Tags.FindAsync(tagId);
            if (tagToDelete == null) 
            {
                throw new Exception("Invalid Id");
            }
            _context.Tags.Remove(tagToDelete);
        }

        public async Task<bool> TagExistsAsync(int tagId)
        {
            return await _context.Tags.AnyAsync(tag=>tag.Id == tagId );
        }

        public void UpdateTag(Tag tagToUpdate)
        {
            _context.Tags.Update(tagToUpdate);
        }
    }
}
