using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Dtos;
using NotesApp.Models;
using NotesApp.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authorization;

namespace NotesApp.Controllers.api
{
    [Authorize]
    [Route("api/{controller}")]
    public class NotesController : ControllerBase
    {
        private readonly UserService _userService;

        public NotesController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("get")]
        public async Task<ActionResult<IEnumerable<Note>>> Get(string Id)
        {
            var user = await _userService.Get(Id);
            var notes = user.Notes;

            return Ok(notes);
        }

        [HttpPost("CreateNote")]
        public async Task<ActionResult<IEnumerable<Note>>> CreateNote([FromBody] CreateNoteDto createNoteDto)
        {
            var newNote = new Note()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Text = createNoteDto.Text,
                CreatedAt = DateTime.Now,
                Complete = false
            };

            await _userService.CreateNote(createNoteDto.UserId, newNote);
            var user = await _userService.Get(createNoteDto.UserId);

            return Ok(user.Notes);
        }

        [HttpPost("DeleteNote")]
        public async Task<ActionResult<IEnumerable<Note>>> DeleteNote([FromBody] DeleteNoteDto deleteNoteDto)
        {
            await _userService.DeleteNote(deleteNoteDto.UserId, deleteNoteDto.NoteId);

            var user = await _userService.Get(deleteNoteDto.UserId);

            return Ok(user.Notes);
        }

        [HttpPut("ToggleNoteComplete")]
        public async Task<ActionResult<IEnumerable<Note>>>
            ToggleNoteComplete([FromBody] ToggleNoteCompleteDto toggleNoteCompleteDto)
        {
            await _userService
                .ToggleNoteComplete(toggleNoteCompleteDto);

            var user = await _userService.Get(toggleNoteCompleteDto.UserId);

            return Ok(user.Notes);
        }
    }
}