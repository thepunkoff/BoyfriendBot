using AutoMapper;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Models;
using System;

namespace BoyfriendBot.Domain.Infrastructure.Mapping
{
    public class DboToDtoProfile : Profile
    {
        public DboToDtoProfile()
        {
            CreateMap<ScheduledMessageDbo, ScheduledMessage>()
            .ForMember(d => d.Guid, c => c.MapFrom(s => Guid.Parse(s.Guid)))
            .ForMember(d => d.Type, c => c.MapFrom(s => Enum.Parse<MessageType>(s.Type)))
            .ForMember(d => d.ChatId, c => c.MapFrom(s => s.ChatId))
            .ForMember(d => d.Time, c => c.MapFrom(s => s.Time));
        }
    }
}
