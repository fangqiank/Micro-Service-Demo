using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlatformService.AsyncDataServices;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlatformController(
        IPlatformRepo repo,
        IMapper mapper,
        ICommandDataClient commandClient,
        IMessageBusClient messageBus)
        : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            Console.WriteLine("-->Getting platforms...");

            var platformItem = repo.GetAllPlatforms();

            return Ok(mapper.Map<IEnumerable<PlatformReadDto>>(platformItem));
        }

        [HttpGet("{id}",Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            var item = repo.GetPlatformById(id);

            if (item != null)
            {
                return Ok(mapper.Map<PlatformReadDto>(item));
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platform)
        {
            var platformTemp = mapper.Map<Platform>(platform);

            repo.CreatePlatform(platformTemp);
            repo.SaveChanges();

            var platformReadDto = mapper.Map<PlatformReadDto>(platformTemp);

            //send sync message
            try
            {
                await commandClient.SendPlatformToCommand(platformReadDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send synchronise: {ex.Message}");
            }

            //send async message
            try
            {
                var platformPublishedDto = mapper.Map<PlatformPublishDto>(platformReadDto) with { Event = "Platform_Published" };

                await messageBus.PublishNewPlatformAsync(platformPublishedDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send asynchronous: {ex.Message}");
            }

            return CreatedAtRoute(nameof(GetPlatformById),new {Id = platformReadDto.Id}, platformReadDto);
        }
    }
}
