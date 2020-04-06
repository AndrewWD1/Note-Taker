using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotesApp.Dtos
{
    public class CreateNoteDto
    {
        public string UserId { get; set; }
        public string Text { get; set; }
    }
}