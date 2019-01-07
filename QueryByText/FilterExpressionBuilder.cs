using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace QueryByText
{
    public interface IFilterExpressionBuilder<T> where T : class
    {
        Expression<Func<T, bool>> TranslateToExpression(string filterStr);
    }

    public class FilterExpressionBuilder<T> : IFilterExpressionBuilder<T> where T : class
    {
        public FilterExpressionBuilder()
        {
            _objType = typeof(T);
            _baseParameter = Expression.Parameter(_objType);
            _visitor = new FilterVisitor(_baseParameter);
        }

        private readonly Type _objType;
        private readonly FilterVisitor _visitor;
        private readonly ParameterExpression _baseParameter;

        public Expression<Func<T, bool>> TranslateToExpression(string filterStr)
        {
            var filterExpr = TranslateToFilterExpression(filterStr);
            var expr = TranslateToExpression(filterExpr);
            return (Expression<Func<T, bool>>)Expression.Lambda(expr, _baseParameter);
        }

        private Expression TranslateToExpression(FilterExpression filterExp)
        {
            return _visitor.Visit(filterExp);
        }

        private FilterExpression TranslateToFilterExpression(string filterStr)
        {
            var splitExp = SplitFilterQuery(filterStr);

            if (splitExp.Length != 3)
            {
                throw new NotSupportedException($"Not supported number of nodes: {splitExp.Length}");
            }

            var exprOne = FilterExpression.Create(splitExp[0], _objType);
            var exprTwo = FilterExpression.Create(splitExp[2], _objType);

            var finalExpression = FilterExpression.Create(exprOne, exprTwo, splitExp[1], _objType);

            return finalExpression;
        }

        private string[] SplitFilterQuery(string filterStr)
        {
            var splitExp = new List<string>();
            var shouldEscape = false;
            var node = "";
            
            // TODO: change to a reqular expression
            for (var i = 0; i < filterStr.Length; i++)
            {
                var ch = filterStr[i];
                
                if (ch == '\'')
                {
                    if (shouldEscape)
                    {
                        splitExp.Add(node);
                        node = "";
                    }
                    shouldEscape = !shouldEscape;

                    continue;
                }

                if (i == filterStr.Length - 1)
                {
                    node += ch;
                }

                if (!shouldEscape && (ch == ' ' || i == filterStr.Length - 1) && node != "")
                {
                    splitExp.Add(node);
                    node = "";
                    continue;
                }

                node += ch;
            }

            return splitExp.ToArray();
        }
    }
}
