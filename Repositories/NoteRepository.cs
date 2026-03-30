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

        public async Task<NoteDTO> Get(int Id)
        {
            Note? note = await _context.Notes.FirstOrDefaultAsync(note => note.Id==Id);

            if(note == null)
            {
                throw new Exception("Invalid Id");
            }

            return MapNoteToNoteDTO(note);
        }

        public async Task<IEnumerable<NoteDTO>?> GetAll()
        {
            return _context.Notes.Select(note => MapNoteToNoteDTO(note)).ToList();
        }

        public async Task Create(Note noteToCreate)
        {
            var note = await _context.Notes.AddAsync(noteToCreate);
            await SaveAsync();
        }

        public async Task Update(NoteDTO noteToUpdate)
        {
            _context.Notes.Update(MapNoteDTONoteTo(noteToUpdate));
            await SaveAsync();
        }

        public async Task Delete(int Id)
        {
            Note? noteToDelete = await _context.Notes.FindAsync(Id);

            if (noteToDelete == null)
            {
                throw new Exception("Invalid Id");
            }

            _context.Notes.Remove(noteToDelete);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        private NoteDTO MapNoteToNoteDTO(Note note)
        {
            return new NoteDTO
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                CharacterCount = note.CharacterCount,
                Tags = note.Tags
            };
        }

        private Note MapNoteDTONoteTo(NoteDTO noteDTO)
        {
            return new Note
            {
                Id = noteDTO.Id,
                Title = noteDTO.Title,
                Content = noteDTO.Content,
                CharacterCount = noteDTO.CharacterCount,
                Tags = noteDTO.Tags
            };
        }
    }
}
