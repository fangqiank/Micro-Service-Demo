using System;
using System.Threading.Tasks;
using PlatformService.Dtos;

namespace PlatformService.AsyncDataServices
{
    public interface IMessageBusClient : IAsyncDisposable
    {
        Task PublishNewPlatformAsync(PlatformPublishDto platform);
    }
}
