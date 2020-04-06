using NotesApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotesApp.Dtos
{
    public class UserWithTokenDto
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public List<Note> Notes { get; set; }
        public string Token { get; set; }
    }
}