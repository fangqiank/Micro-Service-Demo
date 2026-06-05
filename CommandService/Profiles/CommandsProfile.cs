using AutoMapper;
using CommandService.Dtos;
using CommandService.Models;
using PlatformService;

namespace CommandService.Profiles
{
    public class CommandsProfile: Profile
    {
        public CommandsProfile()
        {
            //Source -> Target
            CreateMap<Platform, PlatformReadDto>();
            CreateMap<CommandCreateDto, Command>();
            CreateMap<Command, CommandReadDto>();

            CreateMap<PlatformPublishedDto, Platform>()
                .ForMember(dest => dest.ExternalId,
                    opt => opt.MapFrom(
                        src => src.Id));

            CreateMap<CommandService.Dtos.PlatformReadDto, Platform>()
                .ForMember(dest => dest.ExternalId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Commands,
                    opt => opt.Ignore());

            CreateMap<GrpcPlatformModel, Platform>()
                .ForMember(dest => dest.ExternalId,
                    opt => opt.MapFrom(src => src.PlatformId))
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Commands,
                    opt => opt.Ignore());
        }
    }
}
