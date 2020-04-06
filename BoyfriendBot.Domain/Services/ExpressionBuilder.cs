using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace BoyfriendBot.Domain.Services
{
    public class ExpressionBuilder : IExpressionBuilder
    {
        private readonly IResourceManager _resourceManager;
        private static Dictionary<MatchCategory, Func<string, bool>> _cache;

        static ExpressionBuilder()
        {
            _cache = new Dictionary<MatchCategory, Func<string, bool>>();
        }

        public ExpressionBuilder(
              IResourceManager resourceManager
            )
        {
            _resourceManager = resourceManager;
        }


        public Func<string, bool> BuildCondition(MatchCategory matchCategory)
        {
            if (_cache.ContainsKey(matchCategory))
            {
                return _cache[matchCategory];
            }

            var categoryName = matchCategory.ToString().ToLowerInvariant();

            var matchPatterns = _resourceManager.GetMatchGraph();

            var xMasterCondition = matchPatterns
                .Descendants()
                .Where(x => x.Name.ToString().ToLower() == categoryName)
                .FirstOrDefault()
                .Element("Condition")
                .Elements()
                .FirstOrDefault();

            var conditionExpression = BuildIsMatch(xMasterCondition);

            var func = conditionExpression.Compile();

            _cache.Add(matchCategory, func);

            return func;
        }

        private Expression<Func<string, bool>> BuildIsMatch(XElement xMasterCondition)
        {
            var input = Expression.Parameter(typeof(string), "input");

            var body = Build(xMasterCondition, input);
            
            var lambda = Expression.Lambda<Func<string, bool>>(body, input);

            return lambda;
        }

        private Expression Build(XElement element, ParameterExpression parameter)
        {
            if (element.Name.ToString() == "Or")
            {
                var orExpressions = new List<Expression>();
                var orChildren = element.Elements();
                foreach (var orChild in orChildren)
                {
                    var orExpression = Build(orChild, parameter);
                    orExpressions.Add(orExpression);
                }

                return Or(orExpressions);
            }
            else if (element.Name.ToString() == "And")
            {
                var andExpressions = new List<Expression>();
                foreach (var andChild in element.Elements())
                {
                    var andExpression = Build(andChild, parameter);
                    andExpressions.Add(andExpression);
                }

                return And(andExpressions);
            }
            else if (element.Name.ToString() == "Contains")
            {
                return Expression.Call(parameter, parameter.Type.GetMethod("Contains", new Type[]{ typeof(string) }), new List<Expression>() { Expression.Constant(element.Value) });
            }
            else if (element.Name.ToString() == "ContainsOneOf")
            {
                var anyXParameter = Expression.Parameter(typeof(string), "x");

                var containsCall = Expression.Call(
                           parameter,
                           parameter.Type.GetMethod("Contains", new Type[] { typeof(string) }),
                           new List<Expression>() { anyXParameter });

                var wordList = Expression.Constant(element.Value.Split(", "), typeof(string[]));

                var anyCall = Expression.Call(
                    null,
                    typeof(Enumerable).GetMethods().Where(x => x.Name == "Any").ToList()[1].MakeGenericMethod(new Type[] { typeof(string) }),
                    wordList,
                    Expression.Lambda(
                           body: containsCall,
                           anyXParameter
                ));

                return anyCall;
            }
            else
            {
                throw new Exception("unknown element name");
            }
        }

        private Expression And(List<Expression> andChildren)
        {
            if (andChildren.Count == 2)
            {
                return Expression.And(andChildren[0], andChildren[1]);
            }

            var newList = new List<Expression>(andChildren);

            var and =  Expression.And(newList[0], newList[1]);
            
            newList.RemoveAt(0);
            newList.RemoveAt(0);
            newList.Add(and);

            return And(newList);
        }

        private Expression Or(List<Expression> orChildren)
        {
            if (orChildren.Count == 2)
            {
                return Expression.Or(orChildren[0], orChildren[1]);
            }

            var newList = new List<Expression>(orChildren);

            var or = Expression.Or(newList[0], newList[1]);

            newList.RemoveAt(0);
            newList.RemoveAt(0);
            newList.Add(or);

            return Or(newList);
        }
    }
}