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
    [Route("api/cmd/platforms/{platformId}/[controller]")]
    public class CommandsController(ICommandRepo repo, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
        {
            Console.WriteLine($"--> GetCommandsForPlatform: {platformId}");

            if (!repo.PlatformExists(platformId))
                return NotFound();

            var commands = repo.GetCommandsForPlatform(platformId);

            return Ok(mapper.Map<IEnumerable<CommandReadDto>>(commands));
        }

        [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
        {
            Console.WriteLine($"--> GetCommandForPlatform: {platformId} / {commandId}");

            if (!repo.PlatformExists(platformId))
                return NotFound();

            var command = repo.GetCommand(platformId, commandId);

            if (command is null)
                return NotFound();

            return Ok(mapper.Map<CommandReadDto>(command));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, [FromBody]CommandCreateDto command)
        {
            Console.WriteLine($"--> CreateCommandForPlatform: {platformId} ");

            if (!repo.PlatformExists(platformId))
                return NotFound();

            var commandModel = mapper.Map<Command>(command);

            repo.CreateCommand(platformId, commandModel);
            repo.SaveChanges();

            var commandReadDto = mapper.Map<CommandReadDto>(commandModel);

            return CreatedAtRoute(nameof(GetCommandForPlatform),
                new { platformId = platformId, commandId = commandReadDto.Id },
                commandReadDto);
        }
    }
}
