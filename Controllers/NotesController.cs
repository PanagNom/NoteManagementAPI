using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteManagementAPI.Authorization;
using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;
using NoteManagementAPI.Repositories.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace NoteManagementAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    public class NotesController : ControllerBase
    {
        private const int MaxPageSize = 20;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;

        public NotesController(IUnitOfWork unitOfWork, IMapper mapper, IAuthorizationService authorizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _authorizationService = authorizationService;
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<NoteDTO>>> GetAll(string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1)
            {
                return BadRequest("Page number must be greater than 0.");
            }

            if (pageSize < 1)
            {
                return BadRequest("Page size must be greater than 0.");
            }

            if (pageSize > MaxPageSize)
            {
                pageSize = MaxPageSize;
            }

            var (notes, paginationMetadata) = await _unitOfWork.NoteRepository.GetNotesAsync(GetCurrentUserId(),name, searchQuery, pageNumber, pageSize);

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            return Ok(_mapper.Map<IEnumerable<NoteDTO>>(notes));
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id:int}")]
        public async Task<ActionResult> Get(int id, bool includeTags = false)
        {
            if (id < 1)
            {
                return BadRequest("Id must be greater than 0.");
            }

            Note? retrievedNote = await _unitOfWork.NoteRepository.GetNoteAsync(id, GetCurrentUserId(), includeTags);

            if (retrievedNote == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, retrievedNote, NoteOperations.Read);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            if (includeTags)
            {
                return Ok(_mapper.Map<NoteDTO>(retrievedNote));
            }

            return Ok(_mapper.Map<NoteWithoutTagsDTO>(retrievedNote));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<NoteDTO>> Create(NoteCreationDTO note)
        {
            var noteToCreate = _mapper.Map<Note>(note);
            noteToCreate.AssignOwner(GetCurrentUserId());
            SetCreationAuditFields(noteToCreate);

            var tagResolution = await ResolveTagsAsync(note.Tags);
            if (tagResolution.MissingTagIds.Any())
            {
                return BadRequest($"The following tag ids do not exist: {string.Join(", ", tagResolution.MissingTagIds)}");
            }

            noteToCreate.Tags = tagResolution.Tags;

            await _unitOfWork.NoteRepository.Create(noteToCreate);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = noteToCreate.Id, version = GetRequestedApiVersionValue() }, _mapper.Map<NoteDTO>(noteToCreate));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(int id, NoteUpdateDTO note)
        {
            if (id < 1)
            {
                return BadRequest("Id must be greater than 0.");
            }

            var noteRetrieved = await _unitOfWork.NoteRepository.GetNoteAsync( id, GetCurrentUserId(), includeTags: true);

            if (noteRetrieved == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync( User, noteRetrieved, NoteOperations.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            _mapper.Map(note, noteRetrieved);

            var tagResolution = await ResolveTagsAsync(note.Tags);
            if (tagResolution.MissingTagIds.Any())
            {
                return BadRequest($"The following tag ids do not exist: {string.Join(", ", tagResolution.MissingTagIds)}");
            }

            noteRetrieved.Tags = tagResolution.Tags;
            SetModificationAuditFields(noteRetrieved);

            _unitOfWork.NoteRepository.Update(noteRetrieved);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return BadRequest("Id must be greater than 0.");
            }

            var noteToDelete = await _unitOfWork.NoteRepository.GetNoteAsync(id, GetCurrentUserId());
            if (noteToDelete == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, noteToDelete, NoteOperations.Delete);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            _unitOfWork.NoteRepository.Delete(noteToDelete);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        private async Task<(List<Tag> Tags, List<int> MissingTagIds)> ResolveTagsAsync(IEnumerable<TagInNoteDTO>? tagReferences)
        {
            var tags = new List<Tag>();
            var missingTagIds = new List<int>();
            var ownerUserId = GetCurrentUserId();

            if (tagReferences == null)
            {
                return (tags, missingTagIds);
            }

            foreach (var tagId in tagReferences.Select(tag => tag.Id).Distinct())
            {
                var tagToAdd = await _unitOfWork.TagRepository.GetTagAsync(tagId, ownerUserId);
                if (tagToAdd == null)
                {
                    missingTagIds.Add(tagId);
                    continue;
                }

                tags.Add(tagToAdd);
            }

            return (tags, missingTagIds);
        }

        private void SetCreationAuditFields(Note note)
        {
            var now = DateTime.UtcNow;
            var userName = GetCurrentUserName();

            note.CreatedAt = now;
            note.ModifiedAt = now;
            note.CreatedBy = userName;
            note.ModifiedBy = userName;
        }

        private void SetModificationAuditFields(Note note)
        {
            note.ModifiedAt = DateTime.UtcNow;
            note.ModifiedBy = GetCurrentUserName();
        }

        private string GetCurrentUserName()
        {
            return User.Identity?.Name ?? "unknown";
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException("The authenticated user id claim is missing.");
        }

        private string GetRequestedApiVersionValue()
        {
            return RouteData.Values["version"]?.ToString() ?? "1.0";
        }
    }
}
