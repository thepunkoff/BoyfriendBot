using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;

namespace BoyfriendBot.Domain.Services
{
    public class StringAnalyzer : IStringAnalyzer
    {
        private readonly IExpressionBuilder _expressionBuilder;

        public StringAnalyzer(
              IExpressionBuilder expressionBuilder
            )
        {
            _expressionBuilder = expressionBuilder;
        }

        public bool IsMatch(string input, MatchCategory matchCategory)
        {
            if (matchCategory == MatchCategory.COMMAND)
            {
                return IsCommand(input);
            }
            else
            {
                return IsXmlCondition(input, matchCategory);
            }
        }

        private bool IsCommand(string input)
        {
            return input.StartsWith("/");
        }

        private bool IsXmlCondition(string input, MatchCategory matchCategory)
        {
            var selfieConditionExpression = _expressionBuilder.BuildCondition(matchCategory);

            return selfieConditionExpression(input);
        }
    }
}
