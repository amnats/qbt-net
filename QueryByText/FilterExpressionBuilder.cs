using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

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
            var filter = SplitFilterQuery(filterStr);

            if (filter.Length != 3)
                throw new NotSupportedException("Not supported number of nodes");

            var exprOne = FilterExpression.Create(filter[0], _objType);
            var exprTwo = FilterExpression.Create(filter[2], _objType);

            var finalExpression = FilterExpression.Create(exprOne, exprTwo, filter[1], _objType);

            return finalExpression;
        }

        private string[] SplitFilterQuery(string filterStr)
        {
            // so ugly you should kill yourself lol
            var splitted = new List<string>();
            bool insideQuotes = false;
            string node = "";
            // TODO: change to a reqular expression
            for (int i = 0; i < filterStr.Length; i++)
            {
                char ch = filterStr[i];

                switch (ch)
                {
                    case '\'' when insideQuotes == false:
                        insideQuotes = true;
                        continue;
                    case '\'' when insideQuotes == true:
                        insideQuotes = false;
                        splitted.Add(node);
                        node = "";
                        continue;
                }

                if (i == filterStr.Length - 1)
                    node += ch;

                if (!insideQuotes && (ch == ' ' || i == filterStr.Length - 1) && node != "")
                {
                    splitted.Add(node);
                    node = "";
                    continue;
                }

                node += ch;
            }

            return splitted.ToArray();
        }
    }
}
