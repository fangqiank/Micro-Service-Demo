using System;
using System.Collections.Generic;
using AutoMapper;
using CommandService.Models;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlatformService;

namespace CommandService.SyncDataServices.Grpc
{
    public class PlatformDataClient(IConfiguration configuration, IMapper mapper, ILogger<PlatformDataClient> logger)
        : IPlatformDataClient
    {
        public IEnumerable<Platform> ReturnAllPlatforms()
        {
            logger.LogInformation("--> Calling GRPC Service {GrpcUrl}", configuration["GrpcPlatform"]);

            using var channel = GrpcChannel.ForAddress(configuration["GrpcPlatform"]);
            var client = new GrpcPlatform.GrpcPlatformClient(channel);
            var request = new GetAllRequest();

            const int maxRetries = 5;
            const int delayMs = 3000;

            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var reply = client.GetAllPlatforms(request);
                    return mapper.Map<IEnumerable<Platform>>(reply.Platform);
                }
                catch (RpcException ex) when (attempt < maxRetries)
                {
                    logger.LogWarning("--> GRPC attempt {Attempt}/{MaxRetries} failed: {Detail}. Retrying in {Delay}ms...",
                        attempt, maxRetries, ex.Status.Detail, delayMs);
                    // Synchronous call from startup Configure(); Thread.Sleep is acceptable here
                    System.Threading.Thread.Sleep(delayMs);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "--> Could not call grpc server");
                    return null;
                }
            }

            logger.LogError("--> Could not call grpc server after {MaxRetries} attempts", maxRetries);
            return null;
        }
    }
}
