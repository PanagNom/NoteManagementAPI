using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;
using NoteManagementAPI.Repositories.Interfaces;
using System.Text.Json;
using static Azure.Core.HttpHeader;

namespace NoteManagementAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    public class NotesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public NotesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="searchQuery"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns>All notes created.</returns>
        /// <response code="200">Returns all notes created.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Note>?>> GetAll(string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            if (pageSize > 20) 
            {
                pageSize = 20;
            }

            var (notes, paginationMetadata) = await _unitOfWork.NoteRepository.GetNotesAsync(name, searchQuery, pageNumber, pageSize);

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            return Ok(_mapper.Map<IEnumerable<NoteDTO>>(notes));
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
