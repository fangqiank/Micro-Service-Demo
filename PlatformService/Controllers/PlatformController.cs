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
    public class PlatformController : ControllerBase
    {
        private readonly IPlatformRepo _repo;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandClient;
        private readonly IMessageBusClient _messageBus;

        public PlatformController(
            IPlatformRepo repo, 
            IMapper mapper, 
            ICommandDataClient commandClient, 
            IMessageBusClient messageBus
            )
        {
            _repo = repo;
            _mapper = mapper;
            _commandClient = commandClient;
            _messageBus = messageBus;
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

            var platformReadDto = _mapper.Map<PlatformReadDto>(platformTemp);

            //send sync message
            try
            {
                await _commandClient.SendPlatformToCommand(platformReadDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send synchronise: {ex.Message}");
            }

            //send async message
            try
            {
                var platformPublishedDto = _mapper.Map<PlatformPublishDto>(platformReadDto);
                platformPublishedDto.Event = "Platform_Published";

                _messageBus.PublishNewPlatform(platformPublishedDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send asynchronous: {ex.Message}");
            }

            return CreatedAtRoute(nameof(GetPlatformById),new {Id = platformReadDto.Id}, platformReadDto);
        }
    }
}
