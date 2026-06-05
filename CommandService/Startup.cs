using System;
using CommandService.AsyncDataServices;
using CommandService.Data;
using CommandService.EventProcessing;
using CommandService.SyncDataServices.Grpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace CommandService
{
    public class Startup(IWebHostEnvironment env)
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseInMemoryDatabase("InMemoryDb");
            });

            services.AddScoped<ICommandRepo, CommandRepo>();

            services.AddControllers();

            services.AddHostedService<MessageBusSubscriber>();

            services.AddSingleton<IEventProcessor, EventProcessor>();

            services.AddAutoMapper(cfg => { }, typeof(Startup).Assembly);

            services.AddScoped<IPlatformDataClient, PlatformDataClient>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    if (env.IsDevelopment())
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    }
                    else
                    {
                        policy.AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    }
                });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CommandsService", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors("AllowFrontend");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            PrepDb.PrepPopulation(app);
        }
    }
}
