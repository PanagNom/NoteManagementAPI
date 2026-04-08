using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;
using NoteManagementAPI.Repositories.Interfaces;
using static Azure.Core.HttpHeader;

namespace NoteManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Note>?>> GetAll()
        {
            var notes = await _unitOfWork.NoteRepository.GetAll();
            var noteList = MapNotesToResponse(notes);

            return Ok(noteList);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Note>> Get(int id)
        {
            if (id < 1)
            {
                return BadRequest("Id must be greater than 0");
            }

            Note? retrievedNote = await _unitOfWork.NoteRepository.Get(id);

            if (retrievedNote == null)
            {
                return NotFound();
            }

            return Ok(MapNoteToResponse(retrievedNote));
        }

        [HttpPost]
        public async Task<ActionResult<Note>> Create(NoteDTO note)
        {
            var noteToCreate = MapNoteDTOToNoteCreate(note);
            await _unitOfWork.NoteRepository.Create(noteToCreate);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { Id = noteToCreate.Id }, noteToCreate);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, Note note)
        {
            if (id != note.Id)
            {
                return BadRequest("Id in the URL must match Id in the body");
            }

            _unitOfWork.NoteRepository.Update(note);

            try
            {
                await _unitOfWork.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _unitOfWork.NoteRepository.Exists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _unitOfWork.NoteRepository.Delete(id);

            }
            catch (Exception)
            {
                return NotFound();
            }

            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        private IEnumerable<TagDTO> MapTagsToResponse(IEnumerable<Tag> tag)
        {
            return tag.Select(tag => new TagDTO
            {
                Id = tag.Id,
                Name = tag.Name,
                Notes = tag.Notes
            });
        }

        private IEnumerable<Tag> MapTagsDTOsToTags(IEnumerable<TagDTO> tagDTOs)
        {
            return tagDTOs.Select(tagDTO => new Tag
            {
                Id = tagDTO.Id,
                Name = tagDTO.Name,
                Notes = tagDTO.Notes
            });
        }

        private NoteDTO MapNoteToResponse(Note note)
        {
           return new NoteDTO
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                Tags = MapTagsToResponse(note.Tags)
            };
        }
        private IEnumerable<NoteDTO> MapNotesToResponse(IEnumerable<Note>? notes)
        {
            if(notes == null)
                return Enumerable.Empty<NoteDTO>();

            return notes.Select(note => new NoteDTO
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                Tags = MapTagsToResponse(note.Tags)
            });
        }

        private Note MapNoteDTOToNoteCreate(NoteDTO noteDTO)
        {
            return new Note
            {
                Title = noteDTO.Title,
                Content = noteDTO.Content,
                Tags = MapTagsDTOsToTags(noteDTO.Tags)
            };
        }
        
    }
}
