using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NotesApp.Dtos;
using NotesApp.Models;
using NotesApp.Services;

namespace NotesApp.Controllers
{
    public class UserController : Controller
    {
        private readonly UserService _userService;
        private readonly IConfiguration _config;

        public UserController(UserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }

        [Authorize]
        public async Task<IActionResult> Index(string Id)
        {
            var user = await _userService.Get(Id);
            return View(user);
        }

        public IActionResult SignInForm()
        {
            return View();
        }

        public IActionResult RegisterForm()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromForm] SignInDto signInDto)
        {
            if (!ModelState.IsValid)
                return View("SignInForm", signInDto);

            var userId = await _userService.SignIn(signInDto);

            var user = await _userService.Get(userId);

            if (user == null)
            {
                ViewBag.UserNotFound = true;
                return View("SignInForm");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds,
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var userToSend = new UserWithTokenDto()
            {
                Id = user.Id,
                Name = user.Name,
                Notes = user.Notes,
                Email = user.Email,
                Token = tokenHandler.WriteToken(token)
            };

            return View("Index", userToSend);
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return View("RegisterForm", registerDto);

            var user = await _userService.Register(registerDto);

            if (user == null)
            {
                ViewBag.EmailAlreadyOnFile = registerDto.Email;
                return View("RegisterForm");
            }

            var claims = new[]
{
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds,
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var userToSend = new UserWithTokenDto()
            {
                Id = user.Id,
                Name = user.Name,
                Notes = user.Notes,
                Email = user.Email,
                Token = tokenHandler.WriteToken(token)
            };

            return View("Index", userToSend);
        }
    }
}