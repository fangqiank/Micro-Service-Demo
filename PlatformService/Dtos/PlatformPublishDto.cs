namespace PlatformService.Dtos
{
    public record PlatformPublishDto
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string Event { get; init; }
    }
}
