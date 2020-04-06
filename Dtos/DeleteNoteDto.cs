using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotesApp.Dtos
{
    public class DeleteNoteDto
    {
        public string UserId { get; set; }
        public string NoteId { get; set; }
    }
}