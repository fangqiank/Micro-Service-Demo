using System;
using System.Collections.Generic;
using System.Linq;
using PlatformService.Models;

namespace PlatformService.Data
{
    public class PlatformRepo(AppDbContext ctx) : IPlatformRepo
    {
        public bool SaveChanges()
        {
            return ctx.SaveChanges() >= 0;
        }

        public IEnumerable<Platform> GetAllPlatforms()
        {
            return ctx.Platforms.ToList();
        }

        public Platform GetPlatformById(int id)
        {
            return ctx.Platforms.FirstOrDefault(x => x.Id == id);
        }

        public void CreatePlatform(Platform platform)
        {
            ArgumentNullException.ThrowIfNull(platform);

            ctx.Platforms.Add(platform);
        }
    }
}
