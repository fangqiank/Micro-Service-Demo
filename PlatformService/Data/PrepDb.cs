using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlatformService.Models;

namespace PlatformService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProd);
            }
        }

        private static void SeedData(AppDbContext ctx, bool isProd)
        {
            if (isProd)
            {
                Console.WriteLine("--> Attempting to apply migrations...");
                try
                {
                    ctx.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not run migrations: {ex.Message}");
                }
            }

            if (!ctx.Platforms.Any())
            {
                Console.WriteLine("--> Seeding Data ...");
                ctx.Platforms.AddRange(
                    new Platform{Name = "Dot Net", Publisher = "Microsoft", Cost = "Free"},
                    new Platform{Name = "Sql Server express", Publisher = "Microsoft", Cost = "Free"},
                    new Platform{Name = "Kubernetes", Publisher = "Cloud Native Computing Foundation", Cost = "Free"}
                );

                ctx.SaveChanges();
            }
            else
            {
                Console.WriteLine("We already have data");
            }
        }
    }
}