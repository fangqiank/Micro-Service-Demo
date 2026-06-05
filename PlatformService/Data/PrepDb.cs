using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var ctx = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var commandClient = serviceScope.ServiceProvider.GetService<ICommandDataClient>();
            SeedData(ctx, isProd, commandClient);
        }

        private static void SeedData(AppDbContext ctx, bool isProd, ICommandDataClient commandClient)
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

                // Sync seed data to CommandService via HTTP POST
                if (commandClient != null)
                {
                    foreach (var platform in ctx.Platforms.ToList())
                    {
                        try
                        {
                            commandClient.SendPlatformToCommand(new Dtos.PlatformReadDto
                            {
                                Id = platform.Id,
                                Name = platform.Name,
                                Publisher = platform.Publisher,
                                Cost = platform.Cost
                            }).Wait();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"--> Could not sync seed platform '{platform.Name}': {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("We already have data");
            }
        }
    }
}