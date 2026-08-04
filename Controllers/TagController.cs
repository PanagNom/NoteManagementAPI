using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;
using NoteManagementAPI.Repositories.Interfaces;

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

        public TagController(IUnitOfWork unitOfWork, IMapper map)
        {
            _unitOfWork = unitOfWork;
            _mapper = map;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDTO>>> GetAll()
        {
            var tags = await _unitOfWork.TagRepository.GetTagsAsync();
            return Ok(_mapper.Map<IEnumerable<TagDTO>>(tags));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TagDTO>> Get(int id)
        {
            if (id < 1)
            {
                return BadRequest("Id must be greater than 0.");
            }

            var tag = await _unitOfWork.TagRepository.GetTagAsync(id);

            if (tag == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<TagDTO>(tag));
        }

        [HttpPost]
        public async Task<ActionResult<TagDTO>> Create(TagDTOCreate tag)
        {
            var tagToCreate = _mapper.Map<Tag>(tag);
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

            var tagToUpdate = await _unitOfWork.TagRepository.GetTagAsync(id);

            if (tagToUpdate == null)
            {
                return NotFound();
            }

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
                return BadRequest("Id must be greater than 0");
            }
            var tagToDelete = await _unitOfWork.TagRepository.GetTagAsync(id);
            if (tagToDelete == null)
            {
                return NotFound();
            }
            await _unitOfWork.TagRepository.DeleteTagAsync(id);
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

        private string GetRequestedApiVersionValue()
        {
            return RouteData.Values["version"]?.ToString() ?? "1.0";
        }
    }
}
