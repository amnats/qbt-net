using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace QueryByText
{
    public class FilterExpressionBuilder<T> where T : class
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
            // TODO: change to reqular expression
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

                if(i == filterStr.Length - 1)
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

    public class FilterVisitor
    {
        public FilterVisitor(Expression parameterExpr)
        {
            _parameterExpr = parameterExpr;
        }

        private Expression _parameterExpr;

        public Expression Visit(FilterExpression exp)
        {
            if (exp == null)
                return null;

            switch (exp.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThanOrEqual:
                    return VisitBinary(exp);

                case ExpressionType.MemberAccess:
                    return VisitField(exp);

                case ExpressionType.Constant:
                    return VisitConstant(exp);

                default:
                    throw new NotSupportedException($"Not supported NodeType {exp.NodeType} with value {exp.Value}");
            }
        }

        private Expression VisitBinary(FilterExpression exp)
        {
            var leftVal = Visit(exp.LeftExpr);
            var rightVal = Visit(exp.RightExpr);

            return Expression.MakeBinary(exp.NodeType, leftVal, rightVal);
        }

        private Expression VisitConstant(FilterExpression exp)
        {
            // TODO: add support for other types
            return Expression.Constant(exp.Value, exp.ValueType);
        }

        private Expression VisitField(FilterExpression exp)
        {
            string propName = Helper.FirstToUppercase((string)exp.Value);
            return Expression.MakeMemberAccess(_parameterExpr, _parameterExpr.Type.GetProperty(propName));
        }
    }

    public class FilterExpression
    {
        public FilterExpression LeftExpr { get; }
        public FilterExpression RightExpr { get; }

        public bool IsLeaf => LeftExpr == null && RightExpr == null;

        public ExpressionType NodeType { get; }

        public object Value { get; }
        public Type ValueType { get; }

        public static FilterExpression Create(string nodeString, Type objType)
        {
            return new FilterExpression(null, null, nodeString, objType);
        }

        public static FilterExpression Create(FilterExpression exprOne, FilterExpression exprTwo, string nodeString, Type objType)
        {
            return new FilterExpression(exprOne, exprTwo, nodeString, objType);
        }

        private FilterExpression(FilterExpression leftExpr, FilterExpression rightExpr, string nodeString, Type objType)
        {
            this.LeftExpr = leftExpr;
            this.RightExpr = rightExpr;
            NodeType = SetNodeType(nodeString, objType);

            object value = nodeString;
            ValueType = SetValueType(nodeString, ref value);
            Value = value;
        }

        private ExpressionType SetNodeType(string key, Type objType)
        {
            key = TrimSingleQuotes(key);

            switch (key)
            {
                case "eq":
                    return ExpressionType.Equal;
                case "ne":
                    return ExpressionType.NotEqual;
                case "gt":
                    return ExpressionType.GreaterThan;
                case "lt":
                    return ExpressionType.LessThan;
                case "ge":
                    return ExpressionType.GreaterThanOrEqual;
                case "le":
                    return ExpressionType.LessThanOrEqual;
                default:
                    if (IsObjectField(key, objType))
                        return ExpressionType.MemberAccess;
                    else
                        return ExpressionType.Constant;

            }
        }

        private Type SetValueType(string str, ref object value)
        {
            if (double.TryParse(str, out double val))
            {
                value = val;
                return typeof(double);
            }

            return typeof(string);
        }

        private bool IsObjectField(string key, Type objType)
        {
            bool result = false;

            if (!IsValidPropertyName(key))
                return false;

            string upperCased = Helper.FirstToUppercase(key);

            if (HasProperty(upperCased, objType))
                result = true;
            else if (HasProperty(key, objType))
                result = true;

            return result;
        }

        private bool HasProperty(string propName, Type objType)
        {
            return objType.GetProperties().Any(p => p.Name == propName);
        }

        private bool IsValidPropertyName(string key)
        {
            return key.All(c => AllowedCharacters.Contains(c));
        }

        private List<char> AllowedCharacters
        {
            get
            {
                var allowedCharacters = new List<char>();
                for (char ch = 'A'; ch <= 'z'; ch++)
                {
                    allowedCharacters.Add(ch);
                }
                return allowedCharacters;
            }
        }

        private string TrimSingleQuotes(string str)
        {
            return str.Replace("'", string.Empty);
        }
    }

    public static class Helper
    {
        public static string FirstToUppercase(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }
    }
}
