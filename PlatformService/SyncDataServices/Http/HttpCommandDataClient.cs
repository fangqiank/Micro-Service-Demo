using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PlatformService.Dtos;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace PlatformService.SyncDataServices.Http
{
    public class HttpCommandDataClient: ICommandDataClient
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;

        public HttpCommandDataClient(HttpClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
        }

        public async Task SendPlatformToCommand(PlatformReadDto platform)
        {
            var httpContent = new StringContent(
                JsonSerializer.Serialize(platform),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync($"{_config["CommandService"]}", httpContent);

            Console.WriteLine(response.IsSuccessStatusCode
                ? "--> Sync Post to Command Service was OK!"
                : "--> Sync Post to Command Service was  NOT OK!");
        }
    }
}
