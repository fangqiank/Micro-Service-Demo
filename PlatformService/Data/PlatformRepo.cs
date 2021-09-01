using System;
using System.Collections.Generic;
using System.Linq;
using PlatformService.Models;

namespace PlatformService.Data
{
    public class PlatformRepo: IPlatformRepo
    {
        private readonly AppDbContext _ctx;

        public PlatformRepo(AppDbContext ctx)
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

        public Platform GetPlatformById(int id)
        {
            return _ctx.Platforms.FirstOrDefault(x => x.Id == id);
        }

        public void CreatePlatform(Platform platform)
        {
            if (platform is null)
            {
                throw new ArgumentNullException(nameof(platform));
            }

            _ctx.Platforms.Add(platform);
        }
    }
}
