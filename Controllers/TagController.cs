using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;
using NoteManagementAPI.Repositories.Interfaces;

namespace NoteManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public TagController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>?>> GetAll()
        {
            var tags = await _unitOfWork.TagRepository.GetAll();

            return Ok(MapTagstoTagsDTO(tags));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tag?>> Get(int id)
        {
            var tag = await _unitOfWork.TagRepository.Get(id);

            if (tag == null)
            {
                return NotFound();
            }

            return Ok(MapTagtoTagDTO(tag));
        }

        [HttpPost]
        public async Task<ActionResult<Tag>> Create(TagDTOCreate tag)
        {
            var tagToCreate = MapTagDTOToTagCreate(tag);
            await _unitOfWork.TagRepository.Create(tagToCreate);
            await _unitOfWork.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { Id = tagToCreate.Id }, tagToCreate);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, TagDTO tag)
        {
            if (id < 1)
            {
                return BadRequest("Id must be greater than 0");
            }
            var tagToUpdate = await _unitOfWork.TagRepository.Get(id);
            if (tagToUpdate == null)
            {
                return NotFound();
            }
            tagToUpdate.Name = tag.Name;
            _unitOfWork.TagRepository.Update(tagToUpdate);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return BadRequest("Id must be greater than 0");
            }
            var tagToDelete = await _unitOfWork.TagRepository.Get(id);
            if (tagToDelete == null)
            {
                return NotFound();
            }
            await _unitOfWork.TagRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }

        private Tag MapTagDTOToTagCreate(TagDTOCreate tag)
        {
            return new Tag
            {
                Name = tag.Name,
                CreatedBy = User?.Identity?.Name?? "Tester",
                CreatedDate = DateTime.UtcNow,
                ModifiedBy = User?.Identity?.Name ?? "Tester",
                ModifiedDate = DateTime.UtcNow,
            };
        }

        private IEnumerable<TagDTO> MapTagstoTagsDTO(IEnumerable<Tag>? tags)
        {
            if(tags == null)
            {
                return Enumerable.Empty<TagDTO>();
            }

            return tags.Select(t => new TagDTO
            {
                Id = t.Id,
                Name = t.Name

            });
        }

        private TagDTO MapTagtoTagDTO(Tag tags)
        {
            return new TagDTO
            {
                Id = tags.Id,
                Name = tags.Name,
            };
        }
    }
}
