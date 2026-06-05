using System;
using CommandService.Models;
using System.Collections.Generic;
using System.Linq;

namespace CommandService.Data
{
    public class CommandRepo(AppDbContext ctx) : ICommandRepo
    {
        public bool SaveChanges()
        {
            return ctx.SaveChanges() >= 0;
        }

        public IEnumerable<Platform> GetAllPlatforms()
        {
            return ctx.Platforms.ToList();
        }

        public void CreatePlatform(Platform platform)
        {
            ArgumentNullException.ThrowIfNull(platform);

            ctx.Platforms.Add(platform);
        }

        public bool PlatformExists(int platformId)
        {
            return ctx.Platforms.Any(x => x.Id == platformId);
        }

        public bool ExternalPlatformExists(int externalPlatformId)
        {
            return ctx.Platforms.Any(x => x.ExternalId == externalPlatformId);
        }

        public IEnumerable<Command> GetCommandsForPlatform(int platformId)
        {
            return ctx.Commands
                .Where(x => x.PlatformId == platformId)
                .OrderBy(x => x.Platform.Name);
        }

        public Command GetCommand(int platformId, int commandId)
        {
            return ctx.Commands
                .FirstOrDefault(x => x.PlatformId == platformId 
                                     && x.Id == commandId);
        }

        public void CreateCommand(int platformId, Command command)
        {
            ArgumentNullException.ThrowIfNull(command);

            command.PlatformId = platformId;

            ctx.Commands.Add(command);
        }
    }
}
