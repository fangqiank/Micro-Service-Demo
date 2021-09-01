using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlatformController : ControllerBase
    {
        private readonly IPlatformRepo _repo;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandClient;

        public PlatformController(IPlatformRepo repo, IMapper mapper, ICommandDataClient commandClient)
        {
            _repo = repo;
            _mapper = mapper;
            _commandClient = commandClient;
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
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platform)
        {
            var platformTemp = _mapper.Map<Platform>(platform);

            _repo.CreatePlatform(platformTemp);
            _repo.SaveChanges();

            var platformRead = _mapper.Map<PlatformReadDto>(platformTemp);

            try
            {
                await _commandClient.SendPlatformToCommand(platformRead);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send synchronise: {ex.Message}");
            }

            return CreatedAtRoute(nameof(GetPlatformById),new {Id = platformRead.Id}, platformRead);
        }
    }
}
