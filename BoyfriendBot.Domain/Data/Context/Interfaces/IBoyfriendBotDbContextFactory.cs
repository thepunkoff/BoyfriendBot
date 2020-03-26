namespace BoyfriendBot.Domain.Data.Context.Interfaces
{
    public interface IBoyfriendBotDbContextFactory
    {
        IBoyfriendBotDbContext Create();
    }
}
