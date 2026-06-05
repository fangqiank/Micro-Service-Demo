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
    public class PlatformDataClient : IPlatformDataClient
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<PlatformDataClient> _logger;

        public PlatformDataClient(IConfiguration configuration, IMapper mapper, ILogger<PlatformDataClient> logger)
        {
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
        }

        public IEnumerable<Platform> ReturnAllPlatforms()
        {
            _logger.LogInformation("--> Calling GRPC Service {GrpcUrl}", _configuration["GrpcPlatform"]);

            using var channel = GrpcChannel.ForAddress(_configuration["GrpcPlatform"]);
            var client = new GrpcPlatform.GrpcPlatformClient(channel);
            var request = new GetAllRequest();

            const int maxRetries = 5;
            const int delayMs = 3000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var reply = client.GetAllPlatforms(request);
                    return _mapper.Map<IEnumerable<Platform>>(reply.Platform);
                }
                catch (RpcException ex) when (attempt < maxRetries)
                {
                    _logger.LogWarning("--> GRPC attempt {Attempt}/{MaxRetries} failed: {Detail}. Retrying in {Delay}ms...",
                        attempt, maxRetries, ex.Status.Detail, delayMs);
                    // Synchronous call from startup Configure(); Thread.Sleep is acceptable here
                    System.Threading.Thread.Sleep(delayMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "--> Could not call grpc server");
                    return null;
                }
            }

            _logger.LogError("--> Could not call grpc server after {MaxRetries} attempts", maxRetries);
            return null;
        }
    }
}
