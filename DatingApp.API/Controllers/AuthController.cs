using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        public AuthController(IAuthRepository repo)
        {
            _repo = repo;

        }        

        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string password)
        {
            //TODO: validate request
            
            username = username.ToLower();

            if (await _repo.UserExists(username)) return BadRequest("El nombre de usuario ya existe.");

            //var userToCreate = new User { Username = username };

            var createdUser = await _repo.Register(new User { Username = username }, password);

            return StatusCode(201);
        }
    }
}