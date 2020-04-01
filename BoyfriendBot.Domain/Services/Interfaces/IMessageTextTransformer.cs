using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IMessageTextTransformer
    {
        string ExecuteRant(string rawMessage);
        Task<string> ExecuteInsert(string type, string markedString);

    }
}
