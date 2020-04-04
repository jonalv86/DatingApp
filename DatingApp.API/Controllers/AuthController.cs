using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
            if (await _repo.UserExists(userForRegisterDto.Username)) return BadRequest("El usuario ya existe.");
            var createdUser = await _repo.Register(_mapper.Map<User>(userForRegisterDto), userForRegisterDto.Password);
            return CreatedAtRoute("GetUser", new { controller = "Users", Id = createdUser.Id}, _mapper.Map<UserForDetailedDto>(createdUser));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null) return Unauthorized();

            var tokenHandler = new JwtSecurityTokenHandler();

            return Ok(new
            {
                token = tokenHandler.WriteToken(tokenHandler.CreateToken(new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                        new Claim(ClaimTypes.Name, userFromRepo.Username)
                    }),
                    Expires = DateTime.Now.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value)), SecurityAlgorithms.HmacSha512Signature)
                })),
                user = _mapper.Map<UserForListDto>(userFromRepo)
            });
        }
    }
}