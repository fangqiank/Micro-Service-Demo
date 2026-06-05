using System;
using System.Collections.Generic;
using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controllers
{
    [ApiController]
    [Route("api/cmd/[controller]")]
    public class PlatformController(ICommandRepo repo, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            Console.WriteLine("--> Getting platforms from CommandService");

            var items = repo.GetAllPlatforms();

            return Ok(mapper.Map<IEnumerable<PlatformReadDto>>(items));
        }

        [HttpPost]
        public ActionResult<PlatformReadDto> CreatePlatform(PlatformReadDto platformDto)
        {
            Console.WriteLine($"--> Inbound POST # Command Service (platform sync, Id={platformDto.Id})");

            var platform = mapper.Map<Platform>(platformDto);

            if (!repo.ExternalPlatformExists(platform.ExternalId))
            {
                repo.CreatePlatform(platform);
                repo.SaveChanges();
                Console.WriteLine($"--> Platform synced: ExternalId={platform.ExternalId}, Name={platform.Name}");
                return Ok(mapper.Map<PlatformReadDto>(platform));
            }

            Console.WriteLine($"--> Platform already exists: ExternalId={platform.ExternalId}");
            return Ok(mapper.Map<PlatformReadDto>(platform));
        }
    }
}
