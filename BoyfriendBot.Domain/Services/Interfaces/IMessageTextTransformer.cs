using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IMessageTextTransformer
    {
        string ExecuteRant(string rawMessage, bool gender, bool botGender);
        Task<string> ExecuteInsert(string type, string markedString);

    }
}
