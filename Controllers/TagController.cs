using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteManagementAPI.Authorization;
using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;
using NoteManagementAPI.Repositories.Interfaces;
using System.Security.Claims;

namespace NoteManagementAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    public class TagController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;

        public TagController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IAuthorizationService authorizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDTO>>> GetAll()
        {
            var tags = await _unitOfWork.TagRepository.GetTagsAsync(GetCurrentUserId());
            return Ok(_mapper.Map<IEnumerable<TagDTO>>(tags));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TagDTO>> Get(int id)
        {
            if (id < 1)
            {
                return BadRequest("Id must be greater than 0.");
            }

            var tag = await _unitOfWork.TagRepository.GetTagAsync(id, GetCurrentUserId());
            if (tag == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                tag,
                TagOperations.Read);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return Ok(_mapper.Map<TagDTO>(tag));
        }

        [HttpPost]
        public async Task<ActionResult<TagDTO>> Create(TagDTOCreate tag)
        {
            var ownerUserId = GetCurrentUserId();
            var normalizedName = tag.Name.Trim();
            if (normalizedName.Length == 0)
            {
                return BadRequest("Tag name cannot be empty.");
            }

            if (await _unitOfWork.TagRepository.TagNameExistsAsync(ownerUserId, normalizedName))
            {
                return Conflict("A tag with this name already exists.");
            }

            tag.Name = normalizedName;
            var tagToCreate = _mapper.Map<Tag>(tag);
            tagToCreate.AssignOwner(ownerUserId);
            SetCreationAuditFields(tagToCreate);

            await _unitOfWork.TagRepository.CreateTagAsync(tagToCreate);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = tagToCreate.Id, version = GetRequestedApiVersionValue() },
                _mapper.Map<TagDTO>(tagToCreate));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, TagUpdateDTO tag)
        {
            if (id < 1)
            {
                return BadRequest("Id must be greater than 0.");
            }

            var ownerUserId = GetCurrentUserId();
            var tagToUpdate = await _unitOfWork.TagRepository.GetTagAsync(id, ownerUserId);
            if (tagToUpdate == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                tagToUpdate,
                TagOperations.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var normalizedName = tag.Name.Trim();
            if (normalizedName.Length == 0)
            {
                return BadRequest("Tag name cannot be empty.");
            }

            if (await _unitOfWork.TagRepository.TagNameExistsAsync(ownerUserId, normalizedName, id))
            {
                return Conflict("A tag with this name already exists.");
            }

            tag.Name = normalizedName;
            _mapper.Map(tag, tagToUpdate);
            SetModificationAuditFields(tagToUpdate);

            _unitOfWork.TagRepository.UpdateTag(tagToUpdate);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return BadRequest("Id must be greater than 0.");
            }

            var tagToDelete = await _unitOfWork.TagRepository.GetTagAsync(id, GetCurrentUserId());
            if (tagToDelete == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                tagToDelete,
                TagOperations.Delete);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            _unitOfWork.TagRepository.DeleteTag(tagToDelete);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        private void SetCreationAuditFields(Tag tag)
        {
            var now = DateTime.UtcNow;
            var userName = GetCurrentUserName();

            tag.CreatedDate = now;
            tag.ModifiedDate = now;
            tag.CreatedBy = userName;
            tag.ModifiedBy = userName;
        }

        private void SetModificationAuditFields(Tag tag)
        {
            tag.ModifiedDate = DateTime.UtcNow;
            tag.ModifiedBy = GetCurrentUserName();
        }

        private string GetCurrentUserName()
        {
            return User.Identity?.Name ?? "unknown";
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new InvalidOperationException("The authenticated user id claim is missing.");
        }

        private string GetRequestedApiVersionValue()
        {
            return RouteData.Values["version"]?.ToString() ?? "1.0";
        }
    }
}
