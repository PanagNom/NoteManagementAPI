using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;
using NoteManagementAPI.Repositories.Interfaces;

namespace NoteManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly INoteRepository _noteRepository;

        public NotesController(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NoteDTO>?>> GetAll()
        {
            return Ok(await _noteRepository.GetAll());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<NoteDTO?>> Get(int Id)
        {
            if (Id < 1)
            {
                return BadRequest("Id must be greater than 0");
            }
            return Ok(await _noteRepository.Get(Id));
        }

        [HttpPost]
        public async Task<bool> Create(Note note)
        {
            await _noteRepository.Create(note);
            return true;
        }
    }
}
