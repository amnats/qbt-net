using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QueryByText
{
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

        public static FilterExpression Create(FilterExpression leftExpr, FilterExpression rightExpr, string nodeString, Type objType)
        {
            return new FilterExpression(leftExpr, rightExpr, nodeString, objType);
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
}
