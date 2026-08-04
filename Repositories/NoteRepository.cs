using Microsoft.EntityFrameworkCore;
using NoteManagementAPI.Infrastructure;
using NoteManagementAPI.Models;
using NoteManagementAPI.Repositories.Interfaces;
using NoteManagementAPI.Services;

namespace NoteManagementAPI.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly NoteDbContext _context;

        public NoteRepository(NoteDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Note?> GetNoteAsync(int noteId, string ownerUserId, bool includeTags = false)
        {
            var query = _context.Notes
                .Where(note => note.Id == noteId && note.OwnerUserId == ownerUserId);

            if (includeTags)
            {
                query = query.Include(note => note.Tags);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<(IEnumerable<Note> Notes, PaginationMetadata PaginationMetadata)> GetNotesAsync(
            string ownerUserId,
            string? title,
            string? searchQuery,
            int pageNumber,
            int pageSize)
        {
            IQueryable<Note> notes = _context.Notes
                .AsNoTracking()
                .Where(note => note.OwnerUserId == ownerUserId);

            if (!string.IsNullOrWhiteSpace(title))
            {
                title = title.Trim();
                notes = notes.Where(note => note.Title == title);
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                notes = notes.Where(note =>
                    note.Content.Contains(searchQuery) || note.Title.Contains(searchQuery));
            }

            var totalItemCount = await notes.CountAsync();
            var paginationMetadata = new PaginationMetadata(totalItemCount, pageSize, pageNumber);

            var collectionToReturn = await notes
                .OrderBy(note => note.Title)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return (collectionToReturn, paginationMetadata);
        }

        public async Task Create(Note noteToCreate)
        {
            await _context.Notes.AddAsync(noteToCreate);
        }

        public void Update(Note noteToUpdate)
        {
            _context.Notes.Update(noteToUpdate);
        }

        public void Delete(Note noteToDelete)
        {
            _context.Notes.Remove(noteToDelete);
        }
    }
}
