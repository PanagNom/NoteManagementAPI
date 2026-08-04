using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<Tag>> GetTagsAsync(
            string ownerUserId,
            bool includeNotes = false)
        {
            var query = _context.Tags
                .Where(tag => tag.OwnerUserId == ownerUserId)
                .OrderBy(tag => tag.Name)
                .AsQueryable();

            if (includeNotes)
            {
                query = query.Include(tag => tag.Notes.Where(note => note.OwnerUserId == ownerUserId));
            }

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<Tag?> GetTagAsync(
            int tagId,
            string ownerUserId,
            bool includeNotes = false)
        {
            var query = _context.Tags.Where(tag =>
                tag.Id == tagId && tag.OwnerUserId == ownerUserId);

            if (includeNotes)
            {
                query = query.Include(tag => tag.Notes.Where(note => note.OwnerUserId == ownerUserId));
            }

            return await query.FirstOrDefaultAsync();
        }

        public Task<bool> TagNameExistsAsync(
            string ownerUserId,
            string name,
            int? excludingTagId = null)
        {
            return _context.Tags.AnyAsync(tag =>
                tag.OwnerUserId == ownerUserId &&
                tag.Name == name &&
                (!excludingTagId.HasValue || tag.Id != excludingTagId.Value));
        }

        public async Task CreateTagAsync(Tag tagToCreate)
        {
            await _context.Tags.AddAsync(tagToCreate);
        }

        public void UpdateTag(Tag tagToUpdate)
        {
            _context.Tags.Update(tagToUpdate);
        }

        public void DeleteTag(Tag tagToDelete)
        {
            _context.Tags.Remove(tagToDelete);
        }
    }
}
