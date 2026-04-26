using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;
using NoteManagementAPI.Repositories.Interfaces;

namespace NoteManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapp;
        public TagController(IUnitOfWork unitOfWork, IMapper map)
        {
            _unitOfWork = unitOfWork;
            _mapp = map;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>?>> GetAll()
        {
            var tags = await _unitOfWork.TagRepository.GetTagsAsync();
            return Ok(_mapp.Map<IEnumerable<TagDTO>>(tags));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Tag?>> Get(int id)
        {
            var tag = await _unitOfWork.TagRepository.GetTagAsync(id);

            if (tag == null)
            {
                return NotFound();
            }

            return Ok(_mapp.Map<TagDTO>(tag));
        }

        [HttpPost]
        public async Task<ActionResult<Tag>> Create(TagDTOCreate tag)
        {
            var tagToCreate = _mapp.Map<Tag>(tag);
            baseAttributesFill(tagToCreate);

            await _unitOfWork.TagRepository.CreateTagAsync(tagToCreate);
            await _unitOfWork.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { Id = tagToCreate.Id }, tagToCreate);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, TagUpdateDTO tag)
        {
            var tagToUpdate = await _unitOfWork.TagRepository.GetTagAsync(id);

            if (tagToUpdate == null)
            {
                return NotFound();
            }

            _mapp.Map(tag, tagToUpdate); 

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

        private void baseAttributesFill(Tag tag)
        {
            tag.CreatedDate = DateTime.UtcNow;
            tag.ModifiedDate = DateTime.UtcNow;
            tag.CreatedBy = User.Identity?.Name ?? "CreateTest";
            tag.ModifiedBy = User.Identity?.Name ?? "CreateTest";
        }
    }
}
