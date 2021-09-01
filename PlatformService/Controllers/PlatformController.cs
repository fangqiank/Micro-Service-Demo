using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using System;
using System.Collections.Generic;
using PlatformService.Models;

namespace PlatformService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlatformController : ControllerBase
    {
        private readonly IPlatformRepo _repo;
        private readonly IMapper _mapper;

        public PlatformController(IPlatformRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            Console.WriteLine("-->Getting platforms...");

            var platformItem = _repo.GetAllPlatforms();

            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItem));
        }

        [HttpGet("{id}",Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            var item = _repo.GetPlatformById(id);

            if (item != null)
            {
                return Ok(_mapper.Map<PlatformReadDto>(item));
            }

            return NotFound();
        }

        [HttpPost]
        public ActionResult<PlatformReadDto> CreatePlatform(PlatformCreateDto platform)
        {
            var platformTemp = _mapper.Map<Platform>(platform);

            _repo.CreatePlatform(platformTemp);
            _repo.SaveChanges();

            var platformRead = _mapper.Map<PlatformReadDto>(platformTemp);

            return CreatedAtRoute(nameof(GetPlatformById),new {Id = platformRead.Id}, platformRead);
        }
    }
}
