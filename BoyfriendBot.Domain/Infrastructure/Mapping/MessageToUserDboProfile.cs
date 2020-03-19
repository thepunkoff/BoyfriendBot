using AutoMapper;
using BoyfriendBot.Domain.Data.Models;
using Telegram.Bot.Types;

namespace BoyfriendBot.Domain.Infrastructure.Mapping
{
    public class MessageToUserDboProfile : Profile
    {
        public MessageToUserDboProfile()
        {
           CreateMap<Message, UserDbo>()
           .ForMember(d => d.UserId, c => c.MapFrom(s => s.From.Id))
           .ForMember(d => d.ChatId, c => c.MapFrom(s => s.Chat.Id))
           .ForMember(d => d.FirstName, c => c.MapFrom(s => s.From.FirstName))
           .ForMember(d => d.LastName, c => c.MapFrom(s => s.From.LastName));
        }
    }
}
