using System;
using System.Text.Json;
using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CommandService.EventProcessing
{
    public class EventProcessor(IServiceScopeFactory factory, IMapper mapper) : IEventProcessor
    {
        public void ProcessEvent(string msg)
        {
            var eventType = DetermineEvent(msg);

            switch (eventType)
            {
                case EventType.PlatformPublished:
                    AddPlatform(msg);
                    break;
                case EventType.Undetermined:
                default:
                    break;
            }
        }

        private EventType DetermineEvent(string notification)
        {
            Console.WriteLine("--> Determining event");

            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notification);

            switch (eventType?.Event)
            {
                case "Platform_Published":
                    Console.WriteLine("--> Platform published event detected");
                    return EventType.PlatformPublished;
                default:
                    Console.WriteLine("--> Could not determine the event type");
                    return EventType.Undetermined;
            }
        }

        private void AddPlatform(string platformPublishedMsg)
        {
            using var scope = factory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

            var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMsg);

            try
            {
                var plat = mapper.Map<Platform>(platformPublishedDto);

                if (!repo.ExternalPlatformExists(plat.ExternalId))
                {
                    repo.CreatePlatform(plat);
                    repo.SaveChanges();
                    Console.WriteLine($"--> Platform: {plat} added");
                }
                else
                {
                    Console.WriteLine("--> Platform already exists...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not add platform to DB {ex.Message}");
            }
        }
    }

    enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}
