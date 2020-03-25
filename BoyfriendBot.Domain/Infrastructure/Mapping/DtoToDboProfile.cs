using AutoMapper;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Models;

namespace BoyfriendBot.Domain.Infrastructure.Mapping
{
    public class DtoToDboProfile : Profile
    {
        public DtoToDboProfile()
        {
            CreateMap<ScheduledMessage, ScheduledMessageDbo>()
            .ForMember(d => d.Guid, c => c.MapFrom(s => s.Guid.ToString()))
            .ForMember(d => d.Type, c => c.MapFrom(s => s.Rarity.ToString()))
            .ForMember(d => d.ChatId, c => c.MapFrom(s => s.ChatId))
            .ForMember(d => d.Time, c => c.MapFrom(s => s.Time));
        }
    }
}
