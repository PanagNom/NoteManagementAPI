using Microsoft.EntityFrameworkCore;
using NoteManagementAPI.DTOs;
using NoteManagementAPI.Infrastructure;
using NoteManagementAPI.Models;
using NoteManagementAPI.Repositories.Interfaces;
using NoteManagementAPI.Services;

namespace NoteManagementAPI.Repositories
{
    public class NoteRepository: INoteRepository
    {
        private readonly NoteDbContext _context;

        public NoteRepository(NoteDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Note?> GetNoteAsync(int noteId, bool includeTags=false)
        {
            var query = _context.Notes.Where(note => note.Id == noteId);

            if (includeTags)
            {
                query = query.Include(note => note.Tags);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Note>?> GetNotesAsync()
        {
            return await _context.Notes.OrderBy(note => note.Title).ToListAsync();
        }

        public async Task<(IEnumerable<Note>?, PaginationMetadata)> GetNotesAsync(string? title, string? searchQuery, int pageNumber, int pageSize)
        {
            IQueryable<Note> notes = _context.Notes;

            if (!string.IsNullOrEmpty(title))
            {
                title = title.Trim();
                notes = notes.Where(n => n.Title == title);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                notes = notes.Where(n => n.Content.Contains(searchQuery) || n.Title.Contains(searchQuery));
            }

            var totalItemCount = await notes.CountAsync();
            var paginationMetadata = new PaginationMetadata(totalItemCount, pageSize, pageNumber);

            var collectionToReturn = await notes.OrderBy(n => n.Title)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return (collectionToReturn, paginationMetadata);
        }

        public async Task Create(Note noteToCreate)
        {
            await _context.Notes.AddAsync(noteToCreate);
        }

        public async Task Update(Note noteToUpdate)
        {
            _context.Notes.Update(noteToUpdate);
        }

        public async Task DeleteNote(int noteId)
        {
            Note? noteToDelete = await _context.Notes.FindAsync(noteId);

            if (noteToDelete == null)
            {
                throw new Exception("Invalid Id");
            }

            _context.Notes.Remove(noteToDelete);
        }

        public async Task<bool> NoteExistsAsync(int noteId)
        {
            return await _context.Notes.AnyAsync(note => note.Id == noteId);
        }
    }
}
