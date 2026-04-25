using AutoMapper;
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
        private readonly IMapper _mapper;
        public NotesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Note>?>> GetAll()
        {
            var notes = await _unitOfWork.NoteRepository.GetNotesAsync();

            return Ok(_mapper.Map<IEnumerable<NoteDTO>>(notes));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Note>> Get(int id, bool includeTags = false)
        {
            Note? retrievedNote = await _unitOfWork.NoteRepository.GetNoteAsync(id, includeTags: includeTags);

            if (retrievedNote == null)
            {
                return NotFound();
            }

            if (includeTags)
            {
                return Ok(_mapper.Map<NoteDTO>(retrievedNote));
            }

            return Ok(_mapper.Map<NoteWithoutTagsDTO>(retrievedNote));
        }

        [HttpPost]
        public async Task<ActionResult<Note>> Create(NoteCreationDTO note)
        {
            var noteToCreate = _mapper.Map<Note>(note);
            baseAttributesFill(noteToCreate);
            
            if(note.Tags != null && note.Tags.Any())
            {
                var tagList = note.Tags.ToList();
                List<Tag> noteTags = new List<Tag>();
                foreach (var tag in tagList)
                {
                    var tagToAdd = await _unitOfWork.TagRepository.GetTagAsync(tag.Id);
                    if (tagToAdd != null) 
                    {
                        noteTags.Add(tagToAdd);
                    }
                }
                noteToCreate.Tags = noteTags;
            }
            await _unitOfWork.NoteRepository.Create(noteToCreate);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { Id = noteToCreate.Id }, _mapper.Map<NoteDTO>(noteToCreate));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, NoteUpdateDTO note)
        {
            var noteRetrieved = await _unitOfWork.NoteRepository.GetNoteAsync(id);

            if (noteRetrieved == null)
            {
                return NotFound();
            }

            _mapper.Map(note, noteRetrieved);

            if (note.Tags != null && note.Tags.Any())
            {
                var tagList = note.Tags.ToList();
                List<Tag> noteTags = new List<Tag>();
                foreach (var tag in tagList)
                {
                    var tagToAdd = await _unitOfWork.TagRepository.GetTagAsync(tag.Id);
                    if (tagToAdd != null)
                    {
                        noteTags.Add(tagToAdd);
                    }
                }
                noteRetrieved.Tags = noteTags;
            }
            else
            {
                noteRetrieved.Tags = null;
            }
            await _unitOfWork.NoteRepository.Update(noteRetrieved);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _unitOfWork.NoteRepository.DeleteNote(id);

            }
            catch (Exception)
            {
                return NotFound();
            }

            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    
        private void baseAttributesFill(Note note)
        {
            note.CreatedAt = DateTime.UtcNow;
            note.ModifiedAt = DateTime.UtcNow;
            note.CreatedBy = User.Identity?.Name ?? "CreateTest";
            note.ModifiedBy = User.Identity?.Name ?? "CreateTest";
        }
    }
}
