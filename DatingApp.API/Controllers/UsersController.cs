using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var loggedUser = await _repo.GetUser( int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
            userParams.UserId = loggedUser.Id;
            userParams.Gender = string.IsNullOrEmpty(userParams.Gender) ? (loggedUser.Gender == "male" ? "female" : "male") : userParams.Gender;
            var userPagedList = await _repo.GetUsers(userParams);
            Response.AddPagination(userPagedList.CurrentPage, userPagedList.PageSize, userPagedList.TotalCount, userPagedList.TotalPages);
            return Ok(_mapper.Map<IEnumerable<UserForListDto>>(userPagedList));
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id) => Ok(_mapper.Map<UserForDetailedDto>(await _repo.GetUser(id)));

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) return Unauthorized();
            var userFromRepo = await _repo.GetUser(id);
            _mapper.Map(userForUpdateDto, userFromRepo);
            if (await _repo.SaveAll()) return NoContent();
            throw new Exception($"Updating user {id} failed in save");
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) return Unauthorized();
            if (await _repo.GetLike(id, recipientId) != null) return BadRequest("Ya te gusta ese usuario");
            if (await _repo.GetUser(recipientId) == null) return NotFound();
            _repo.Add<Like>(new Like
            {
                LikerId = id,
                LikeeId = recipientId
            });
            if (await _repo.SaveAll()) return Ok();
            return BadRequest("Ocurrió un error al likear al usuario");
        }
    }
}