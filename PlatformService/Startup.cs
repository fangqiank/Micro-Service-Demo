using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;

namespace PlatformService
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (_env.IsProduction())
            {
                Console.WriteLine("--> Using PostgreSQL Db");
                services.AddDbContext<AppDbContext>(opt =>
                {
                    opt.UseNpgsql(Configuration.GetConnectionString("PlatformsConn"));
                });
            }
            else
            {
                Console.WriteLine("--> Using InMemory Db");
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemory");
                });
            }

            services.AddScoped<IPlatformRepo, PlatformRepo>();

            services.AddSingleton<IMessageBusClient, MessageBusClient>();

            services.AddGrpc();

            services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();

            services.AddControllers();

            services.AddAutoMapper(cfg => { }, typeof(Startup).Assembly);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PlatformService", Version = "v1" });
            });

            Console.WriteLine($"--> Command Service Endpoint {Configuration["CommandService"]}");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PlatformService v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<GrpcPlatformService>();

                endpoints.MapGet("/protos/platforms.proto", async ctx =>
                {
                    await ctx.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
                });
            });

            PrepDb.PrepPopulation(app, env.IsProduction());
        }
    }
}
