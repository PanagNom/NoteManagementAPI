using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteManagementAPI.Models;
using NoteManagementAPI.Repositories.Interfaces;

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
            return Ok(await _unitOfWork.NoteRepository.GetAll());
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

            return retrievedNote;
        }

        [HttpPost]
        public async Task<ActionResult<Note>> Create(Note note)
        {
            await _unitOfWork.NoteRepository.Create(note);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { Id = note.Id }, note);
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
    }
}
