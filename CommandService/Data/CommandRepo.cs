using System;
using CommandService.Models;
using System.Collections.Generic;
using System.Linq;

namespace CommandService.Data
{
    public class CommandRepo: ICommandRepo
    {
        private readonly AppDbContext _ctx;

        public CommandRepo(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public bool SaveChanges()
        {
            return _ctx.SaveChanges() >= 0;
        }

        public IEnumerable<Platform> GetAllPlatforms()
        {
            return _ctx.Platforms.ToList();
        }

        public void CreatePlatform(Platform platform)
        {
            if (platform == null)
            {
                throw new ArgumentNullException(nameof(platform));
            }

            _ctx.Platforms.Add(platform);
        }

        public bool PlatformExists(int platformId)
        {
            return _ctx.Platforms.Any(x => x.Id == platformId);
        }

        public bool ExternalPlatformExists(int externalPlatformId)
        {
            return _ctx.Platforms.Any(x => x.ExternalId == externalPlatformId);
        }

        public IEnumerable<Command> GetCommandsForPlatform(int platformId)
        {
            return _ctx.Commands
                .Where(x => x.PlatformId == platformId)
                .OrderBy(x => x.Platform.Name);
        }

        public Command GetCommand(int platformId, int commandId)
        {
            return _ctx.Commands
                .FirstOrDefault(x => x.PlatformId == platformId 
                                     && x.Id == commandId);
        }

        public void CreateCommand(int platformId, Command command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            command.PlatformId = platformId;

            _ctx.Commands.Add(command);
        }
    }
}
