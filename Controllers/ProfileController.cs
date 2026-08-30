using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FrameForge.Data;
using FrameForge.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrameForge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly FrameForgeDbContext _context;

        public ProfileController(
            FrameForgeDbContext context)
        {
            _context = context;
        }


        // GET: api/Profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier
            );

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(
                userIdClaim.Value
            );

            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.UserId == userId
                );

            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return Ok(userDto);
        }


        // PUT: api/Profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileDto updateProfileDto)
        {
            var userIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier
            );

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(
                updateProfileDto.Name
            ))
            {
                return BadRequest(
                    new
                    {
                        message = "Name is required."
                    }
                );
            }

            if (string.IsNullOrWhiteSpace(
                updateProfileDto.Email
            ))
            {
                return BadRequest(
                    new
                    {
                        message = "Email is required."
                    }
                );
            }

            var userId = int.Parse(
                userIdClaim.Value
            );

            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.UserId == userId
                );

            if (user == null)
            {
                return NotFound();
            }

            var email = updateProfileDto.Email
                .Trim()
                .ToLower();

            var emailExists = await _context.Users
                .AnyAsync(u =>
                    u.Email.ToLower() == email &&
                    u.UserId != userId
                );

            if (emailExists)
            {
                return BadRequest(
                    new
                    {
                        message =
                            "This email is already being used."
                    }
                );
            }

            user.Name = updateProfileDto.Name.Trim();
            user.Email = email;

            await _context.SaveChangesAsync();

            return Ok(
                new UserDto
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            );
        }
    }
}