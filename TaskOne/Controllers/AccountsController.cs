using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TaskOne.Data;
using TaskOne.DTOs;
using TaskOne.Entities;

namespace TaskOne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly DataContext _context;

        public AccountsController(DataContext context)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
        }

        //Not for the task, Just for me
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email);

            if (user == null) return NotFound("Fuck you");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
                if (computedHash[i] != user.PasswordHash[i])
                    return NotFound("Fuck you");

            return Ok("Welcome");
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Email)) return BadRequest("Email is taken");

            using var hmac = new HMACSHA512();

            var user = new User
            {
                Email = registerDto.Email.ToLower(),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Created");
        }

        private async Task<bool> UserExists(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email == email.ToLower());
        }


    }
}
