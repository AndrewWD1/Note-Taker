using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotesApp.Models;
using NotesApp.Dtos;
using MongoDB.Bson;

namespace NotesApp.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _users = database.GetCollection<User>(settings.UsersCollectionName);
        }

        public List<User> Get() =>
            _users.Find(user => true).ToList();

        public async Task<User> Get(string Id)
        {
            var users = await _users.FindAsync(user => user.Id == Id);
            var user = await users.SingleOrDefaultAsync();

            return user;
        }

        public async Task<string> SignIn(SignInDto signInDto)
        {
            var users = await _users.FindAsync(user => user.Email == signInDto.Email);
            var user = await users.SingleOrDefaultAsync();

            if (user == null)
                return null;

            if (!VerifyPasswordHash(signInDto.Password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user.Id;
        }

        public async Task<User> Register(RegisterDto registerDto)
        {
            var users = await _users.FindAsync(user => user.Email == registerDto.Email);
            var user = await users.SingleOrDefaultAsync();

            if (user != null)
                return null;

            var newUser = new User()
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Notes = new List<Note>()
                {
                    new Note()
                    {
                        Text = "First Note",
                        CreatedAt = DateTime.Now,
                        Complete = true
                    }
                }
            };

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(registerDto.Password, out passwordHash, out passwordSalt);

            newUser.PasswordHash = passwordHash;
            newUser.PasswordSalt = passwordSalt;

            await _users.InsertOneAsync(newUser);

            return newUser;
        }

        public async Task ToggleNoteComplete(ToggleNoteCompleteDto toggleNoteCompleteDto)
        {
            var update = Builders<User>.Update.Set(x => x.Notes[-1].Complete, toggleNoteCompleteDto.NewToggleState);

            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(x => x.Id, toggleNoteCompleteDto.UserId),
                Builders<User>.Filter.ElemMatch(x => x.Notes, x => x.Id == toggleNoteCompleteDto.NoteId));

            await _users.FindOneAndUpdateAsync<User>(filter, update);
        }

        public async Task CreateNote(string userId, Note note)
        {
            var update = Builders<User>.Update.Push("Notes", note);

            await _users.FindOneAndUpdateAsync<User>(user => user.Id == userId, update);
        }

        public async Task DeleteNote(string userId, string noteId)
        {
            var filter = Builders<Note>.Filter.Eq("Id", noteId);
            var update = Builders<User>.Update.PullFilter("Notes", filter);

            await _users.FindOneAndUpdateAsync(user => user.Id == userId, update);
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }

            return true;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}